using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Aggregate Root que orquestra todo o modelo e fluxo da Ordem de Serviço.
/// </summary>
public sealed class OrdemServico
{
    public Guid Id { get; private set; }

    // Configurado no EF para nunca ser null ao instanciar via ORM. Initialize-lo previne warnings em propriedades `required`.
    public NumeroOS Numero { get; private set; } = default!;
    public StatusOS Status { get; private set; }

    public Guid ClienteId { get; private set; }
    public Guid? EquipamentoId { get; private set; }

    public string Defeito { get; private set; } = string.Empty;
    public string? LaudoTecnico { get; private set; }
    public string? Observacoes { get; private set; }
    public string? CondicoesPagamento { get; private set; }
    public string? Referencia { get; private set; }
    public string? Duracao { get; private set; }

    public DateOnly? ValidadeOrcamento { get; private set; }
    public DateOnly? PrazoEntrega { get; private set; }

    public Desconto? DescontoAplicado { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Aggregate members. São expostos como IReadOnlyCollection e encapsulados no Backing Field de list para o compilador do ORM.
    private readonly List<OrdemServicoServico> _servicos = new();
    public IReadOnlyCollection<OrdemServicoServico> Servicos => _servicos.AsReadOnly();

    private readonly List<OrdemServicoProduto> _produtos = new();
    public IReadOnlyCollection<OrdemServicoProduto> Produtos => _produtos.AsReadOnly();

    private readonly List<OrdemServicoTaxa> _taxas = new();
    public IReadOnlyCollection<OrdemServicoTaxa> Taxas => _taxas.AsReadOnly();

    private readonly List<OrdemServicoPagamento> _pagamentos = new();
    public IReadOnlyCollection<OrdemServicoPagamento> Pagamentos => _pagamentos.AsReadOnly();

    private readonly List<OrdemServicoFoto> _fotos = new();
    public IReadOnlyCollection<OrdemServicoFoto> Fotos => _fotos.AsReadOnly();

    private readonly List<OrdemServicoAnotacao> _anotacoes = new();
    public IReadOnlyCollection<OrdemServicoAnotacao> Anotacoes => _anotacoes.AsReadOnly();

    // EF Core constructor
    private OrdemServico() { }

    public static OrdemServico Criar(
        Guid clienteId,
        Guid? equipamentoId,
        string defeito,
        string? duracao,
        string? observacoes,
        string? referencia,
        DateOnly? validadeOrcamento,
        DateOnly? prazoEntrega)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("Cliente é obrigatório.", nameof(clienteId));

        if (string.IsNullOrWhiteSpace(defeito))
            throw new ArgumentException("Descrição do defeito é obrigatória.", nameof(defeito));

        var momentoCadastro = DateTime.UtcNow;

        return new OrdemServico
        {
            Id = Guid.NewGuid(),
            // Número será setado no momento de inclusão com a sequência e data via Repositório/DB. Default temporário.
            Status = StatusOS.Rascunho,
            ClienteId = clienteId,
            EquipamentoId = equipamentoId,
            Defeito = defeito,
            Duracao = duracao,
            Observacoes = observacoes,
            Referencia = referencia,
            ValidadeOrcamento = validadeOrcamento,
            PrazoEntrega = prazoEntrega,
            CreatedAt = momentoCadastro,
            UpdatedAt = momentoCadastro
        };
    }

    /// <summary>
    /// Configura o ID da OS após verificação de contagem sequencial
    /// </summary>
    public void DefinirNumeroOS(NumeroOS numero)
    {
        if (Numero is not null && !string.IsNullOrEmpty(Numero.Valor))
            throw new DomainException("Ordem de Serviço já possui um número emitido.");

        Numero = numero;
    }

    // Comportamentos e regras de Domínio

    public void AtualizarDadosBasicos(string defeito, string? duracao, string? observacoes, string? referencia, DateOnly? validade, DateOnly? prazo)
    {
        if (Status is StatusOS.Aprovada or StatusOS.EmAndamento or StatusOS.Concluida or StatusOS.Entregue)
            throw new DomainException($"Não é mais permitido alterar dados básicos quando ela já está {Status}.");

        if (string.IsNullOrWhiteSpace(defeito))
            throw new ArgumentException("Descrição do defeito não pode ficar vazia.", nameof(defeito));

        Defeito = defeito;
        Duracao = duracao;
        Observacoes = observacoes;
        Referencia = referencia;
        ValidadeOrcamento = validade;
        PrazoEntrega = prazo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegistrarLaudoTecnico(string laudo)
    {
        if (string.IsNullOrWhiteSpace(laudo))
            throw new ArgumentException("Laudo deve ter um conteúdo.", nameof(laudo));

        LaudoTecnico = laudo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AplicarDesconto(Desconto desconto)
    {
        if (!PodeModificarFinanceiro())
            throw new DomainException("Não é possível alterar taxas e descontos com o status atual.");

        var subtotal = _servicos.Sum(x => x.Subtotal) + _produtos.Sum(x => x.Subtotal);
        desconto.CalcularValorEfetivo(subtotal); // Validará se excede total. Lança Exception se sim.

        DescontoAplicado = desconto;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdicionarServico(string descricao, int quantidade, decimal valorUnitario)
    {
        if (!PodeModificarFinanceiro())
            throw new DomainException("As modificações financeiras não são permitidas neste Status.");

        var ordem = _servicos.Count + 1;
        _servicos.Add(OrdemServicoServico.Criar(Id, descricao, quantidade, valorUnitario, ordem));
        VerificarIntegridadeDeDesconto();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdicionarProduto(string descricao, int quantidade, decimal valorUnitario)
    {
        if (!PodeModificarFinanceiro())
            throw new DomainException("As modificações financeiras não são permitidas neste Status.");

        var ordem = _produtos.Count + 1;
        _produtos.Add(OrdemServicoProduto.Criar(Id, descricao, quantidade, valorUnitario, ordem));
        VerificarIntegridadeDeDesconto();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdicionarTaxa(string descricao, decimal valor)
    {
        if (!PodeModificarFinanceiro())
            throw new DomainException("As modificações financeiras não são permitidas neste Status.");

        _taxas.Add(OrdemServicoTaxa.Criar(Id, descricao, valor));
        VerificarIntegridadeDeDesconto();
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdicionarAnotacao(string texto, string autor)
    {
        _anotacoes.Add(OrdemServicoAnotacao.Criar(Id, texto, autor));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdicionarPagamento(MeioPagamento meio, decimal valor, DateTime dataPagamento)
    {
        var total = CalcularTotalReal();
        var jaVigente = _pagamentos.Sum(p => p.Valor);

        if (jaVigente + valor > total.Valor)
            throw new DomainException("O valor do pagamento vai exceder o total calculado para a Ordem de Serviço.");

        _pagamentos.Add(OrdemServicoPagamento.Criar(Id, meio, valor, dataPagamento));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdicionarFoto(string url, string? legenda)
    {
        _fotos.Add(OrdemServicoFoto.Criar(Id, url, legenda));
        UpdatedAt = DateTime.UtcNow;
    }

    // Engine de Totalização
    public Dinheiro CalcularTotalReal()
    {
        var sumServicos = _servicos.Sum(s => s.Subtotal);
        var sumProdutos = _produtos.Sum(p => p.Subtotal);
        var sumTaxas = _taxas.Sum(t => t.Valor);
        var subtotal = sumServicos + sumProdutos;

        var valDesconto = DescontoAplicado?.CalcularValorEfetivo(subtotal) ?? 0m;

        var baseDinheiroEfetivo = subtotal + sumTaxas - valDesconto;
        return new Dinheiro(baseDinheiroEfetivo);
    }

    private void VerificarIntegridadeDeDesconto()
    {
        if (DescontoAplicado is null) return;

        var subtotal = _servicos.Sum(x => x.Subtotal) + _produtos.Sum(x => x.Subtotal);
        try
        {
            DescontoAplicado.CalcularValorEfetivo(subtotal);
        }
        catch (DescontoExcedeTotalException)
        {
            // O desconto anterior ficou incompatível com a remoção de serviços. Removendo.
            DescontoAplicado = Desconto.Nenhum;
        }
    }

    private bool PodeModificarFinanceiro() =>
        Status is StatusOS.Rascunho or StatusOS.Orcamento or StatusOS.Aprovada;

    // Transição de Status Workflow

    public void MarcarComoOrcamento() => EfetuarTransicaoWorkflow(StatusOS.Orcamento, StatusOS.Rascunho);
    public void Aprovar() => EfetuarTransicaoWorkflow(StatusOS.Aprovada, StatusOS.Rascunho, StatusOS.Orcamento);
    public void Rejeitar() => EfetuarTransicaoWorkflow(StatusOS.Rejeitada, StatusOS.Orcamento);
    public void IniciarAndamento() => EfetuarTransicaoWorkflow(StatusOS.EmAndamento, StatusOS.Aprovada, StatusOS.AguardandoPeca);
    public void PausarAguardandoPeca() => EfetuarTransicaoWorkflow(StatusOS.AguardandoPeca, StatusOS.EmAndamento);
    public void ConcluirTrabalho() => EfetuarTransicaoWorkflow(StatusOS.Concluida, StatusOS.EmAndamento);
    public void FinalizarEEntregar()
    {
        var total = CalcularTotalReal();
        var jaVigente = _pagamentos.Sum(p => p.Valor);

        if (total.Valor > jaVigente)
            throw new DomainException($"O pagamento precisa estar finalizado. Faltam {total.Valor - jaVigente:C} para cobrir o serviço.");

        EfetuarTransicaoWorkflow(StatusOS.Entregue, StatusOS.Concluida, StatusOS.Rejeitada);
    }

    private void EfetuarTransicaoWorkflow(StatusOS desejado, params StatusOS[] permitidos)
    {
        if (!permitidos.Contains(Status) && Status != desejado)
            throw new StatusTransicaoInvalidaException(Status, desejado);

        Status = desejado;
        UpdatedAt = DateTime.UtcNow;
    }
}
