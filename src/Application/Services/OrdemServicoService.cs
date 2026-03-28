using Application.DTOs.Comuns;
using Application.DTOs.OrdemServicos;
using Application.DTOs.OrdemServicos.Mappings;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.ValueObjects;

namespace Application.Services;

public sealed class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoRepository _osRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrdemServicoService(IOrdemServicoRepository osRepository, IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
    {
        _osRepository = osRepository;
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrdemServicoResponse> CriarAsync(CriarOrdemServicoRequest request, CancellationToken cancellationToken = default)
    {
        var cliente = await _clienteRepository.ObterPorIdAsync(request.ClienteId, cancellationToken);
        if (cliente is null) throw new DomainException("Cliente não encontrado.");

        var os = OrdemServico.Criar(
            request.ClienteId,
            request.EquipamentoId,
            request.Defeito,
            request.Duracao,
            request.Observacoes,
            request.Referencia,
            request.ValidadeOrcamento,
            request.PrazoEntrega
        );

        // Gera o Número Sequencial (Ex: OS-20260307-0001) bloqueando na DB de forma segura.
        var seq = await _osRepository.ObterProximoSequencialNoDiaAsync(DateTime.UtcNow, cancellationToken);
        var numeroOs = NumeroOS.Gerar(DateTime.UtcNow, seq);
        os.DefinirNumeroOS(numeroOs);

        await _osRepository.AdicionarAsync(os, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return os.ToResponse();
    }

    public async Task AtualizarAsync(Guid id, AtualizarOrdemServicoRequest request, CancellationToken cancellationToken = default)
    {
        var os = await _osRepository.ObterPorIdAsync(id, cancellationToken);
        if (os is null) throw new DomainException("OS não encontrada.");

        os.AtualizarDadosBasicos(request.Defeito, request.Duracao, request.Observacoes, request.Referencia, request.ValidadeOrcamento, request.PrazoEntrega);

        await _osRepository.AtualizarAsync(os, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task<OrdemServicoResponse?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var os = await _osRepository.ObterPorIdAsync(id, cancellationToken);
        return os?.ToResponse();
    }

    public async Task<PagedResponse<OrdemServicoResumoResponse>> ListarPaginadoAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        var itens = await _osRepository.ListarPaginadoAsync(request.Page, request.PageSize, cancellationToken);
        var totalCount = await _osRepository.ContarAsync(cancellationToken);
        var listResponse = itens.Select(i => i.ToResumoResponse());
        return new PagedResponse<OrdemServicoResumoResponse>(listResponse, totalCount, request.Page, request.PageSize);
    }

    public async Task AdicionarServicoAsync(Guid id, AdicionarServicoRequest request, CancellationToken cancellationToken = default)
    {
        var os = await GetOsOuThrow(id, cancellationToken);
        os.AdicionarServico(request.Descricao, request.Quantidade, request.ValorUnitario);
        await Guardar(os, cancellationToken);
    }

    public async Task AdicionarProdutoAsync(Guid id, AdicionarProdutoRequest request, CancellationToken cancellationToken = default)
    {
        var os = await GetOsOuThrow(id, cancellationToken);
        os.AdicionarProduto(request.Descricao, request.Quantidade, request.ValorUnitario);
        await Guardar(os, cancellationToken);
    }

    public async Task AplicarDescontoAsync(Guid id, AplicarDescontoRequest request, CancellationToken cancellationToken = default)
    {
        var os = await GetOsOuThrow(id, cancellationToken);
        var desconto = new Desconto(request.Tipo, request.Valor);
        os.AplicarDesconto(desconto);
        await Guardar(os, cancellationToken);
    }

    public async Task AdicionarTaxaAsync(Guid id, AdicionarTaxaRequest request, CancellationToken cancellationToken = default)
    {
        var os = await GetOsOuThrow(id, cancellationToken);
        os.AdicionarTaxa(request.Descricao, request.Valor);
        await Guardar(os, cancellationToken);
    }

    public async Task RegistrarPagamentoAsync(Guid id, RegistrarPagamentoRequest request, CancellationToken cancellationToken = default)
    {
        var os = await GetOsOuThrow(id, cancellationToken);

        ValidateConcurrency(os, request.ExpectedUpdatedAt);

        var pagamentoDuplicado = os.Pagamentos.Any(p =>
            p.MeioPagamento == request.Meio
            && p.Valor == request.Valor
            && p.DataPagamento == request.DataPagamento);

        if (pagamentoDuplicado)
        {
            // Idempotencia: a mesma requisicao de pagamento nao deve gerar duplicidade.
            return;
        }

        os.AdicionarPagamento(request.Meio, request.Valor, request.DataPagamento);
        await Guardar(os, cancellationToken);
    }

    public async Task AdicionarAnotacaoAsync(Guid id, AdicionarAnotacaoRequest request, CancellationToken cancellationToken = default)
    {
        var os = await GetOsOuThrow(id, cancellationToken);
        os.AdicionarAnotacao(request.Texto, request.Autor);
        await Guardar(os, cancellationToken);
    }

    public async Task AlterarStatusAsync(Guid id, AlterarStatusRequest request, CancellationToken cancellationToken = default)
    {
        var os = await GetOsOuThrow(id, cancellationToken);

        ValidateConcurrency(os, request.ExpectedUpdatedAt);

        if (os.Status == request.NovoStatus)
        {
            // Idempotencia: repetir a mesma transicao nao deve causar efeitos colaterais.
            return;
        }

        switch (request.NovoStatus)
        {
            case StatusOS.Orcamento: os.MarcarComoOrcamento(); break;
            case StatusOS.Aprovada: os.Aprovar(); break;
            case StatusOS.Rejeitada: os.Rejeitar(); break;
            case StatusOS.EmAndamento: os.IniciarAndamento(); break;
            case StatusOS.AguardandoPeca: os.PausarAguardandoPeca(); break;
            case StatusOS.Concluida: os.ConcluirTrabalho(); break;
            case StatusOS.Entregue: os.FinalizarEEntregar(); break;
            default: break;
        }

        await Guardar(os, cancellationToken);
    }

    private async Task<OrdemServico> GetOsOuThrow(Guid id, CancellationToken ct)
    {
        var os = await _osRepository.ObterPorIdAsync(id, ct);
        if (os is null) throw new DomainException("OS não encontrada.");
        return os;
    }

    private async Task Guardar(OrdemServico os, CancellationToken ct)
    {
        await _osRepository.AtualizarAsync(os, ct);
        await _unitOfWork.CommitAsync(ct);
    }

    private static void ValidateConcurrency(OrdemServico os, DateTime? expectedUpdatedAt)
    {
        if (!expectedUpdatedAt.HasValue)
        {
            return;
        }

        var expected = DateTime.SpecifyKind(expectedUpdatedAt.Value, DateTimeKind.Utc);
        var persisted = DateTime.SpecifyKind(os.UpdatedAt, DateTimeKind.Utc);

        if (persisted > expected)
        {
            throw new ConcurrencyConflictException(
                "A Ordem de Servico foi alterada por outro processo. Recarregue a tela e tente novamente.");
        }
    }
}
