using System.Text.Json;
using System.Text.Json.Nodes;
using Web.Models;
using Web.Models.Requests;
using Web.Models.Responses;
using Web.Services.Api;
using Web.State;
using Web.ViewModels.Foundation;

namespace Web.ViewModels.OrdensServico;

public sealed class OrdemServicoDetalheViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IOrdensServicoApi _ordensServicoApi;
    private readonly OrdemServicoFlowState _ordemServicoFlowState;

    private Guid? _ordemServicoId;
    private OrdemServicoResponseModel? _detalhe;
    private IReadOnlyDictionary<string, string[]> _validationErrors = new Dictionary<string, string[]>();

    public OrdemServicoDetalheViewModel(IOrdensServicoApi ordensServicoApi, OrdemServicoFlowState ordemServicoFlowState)
    {
        _ordensServicoApi = ordensServicoApi;
        _ordemServicoFlowState = ordemServicoFlowState;
    }

    public Guid? OrdemServicoId
    {
        get => _ordemServicoId;
        private set => SetProperty(ref _ordemServicoId, value);
    }

    public OrdemServicoResponseModel? Detalhe
    {
        get => _detalhe;
        private set => SetProperty(ref _detalhe, value);
    }

    public IReadOnlyDictionary<string, string[]> ValidationErrors
    {
        get => _validationErrors;
        private set => SetProperty(ref _validationErrors, value);
    }

    public OrdemServicoComposicaoItemFormModel ServicoForm { get; private set; } = new();
    public OrdemServicoComposicaoItemFormModel ProdutoForm { get; private set; } = new();
    public OrdemServicoTaxaFormModel TaxaForm { get; private set; } = new();
    public OrdemServicoDescontoFormModel DescontoForm { get; private set; } = new();
    public OrdemServicoAnotacaoFormModel AnotacaoForm { get; private set; } = new();
    public OrdemServicoPagamentoFormModel PagamentoForm { get; private set; } = new();

    public IReadOnlyList<TipoDescontoModel> TiposDesconto { get; } = Enum.GetValues<TipoDescontoModel>();

    public IReadOnlyList<StatusOsModel> StatusTimelineOrder { get; } =
    [
        StatusOsModel.Rascunho,
        StatusOsModel.Orcamento,
        StatusOsModel.Aprovada,
        StatusOsModel.EmAndamento,
        StatusOsModel.AguardandoPeca,
        StatusOsModel.Concluida,
        StatusOsModel.Entregue,
        StatusOsModel.Rejeitada
    ];

    public bool PodeEnviarOrcamento => Detalhe?.Status == StatusOsModel.Rascunho;
    public bool IsEdicaoBloqueadaAposEnvio => Detalhe is not null && Detalhe.Status != StatusOsModel.Rascunho;
    public bool PodeAprovar => Detalhe?.Status == StatusOsModel.Orcamento;
    public bool PodeRejeitar => Detalhe?.Status == StatusOsModel.Orcamento;
    public bool PodeIniciarAndamento => Detalhe is not null && (Detalhe.Status == StatusOsModel.Aprovada || Detalhe.Status == StatusOsModel.AguardandoPeca);
    public bool PodeConcluir => Detalhe?.Status == StatusOsModel.EmAndamento;
    public bool PodeEntregar => Detalhe is not null && (Detalhe.Status == StatusOsModel.Concluida || Detalhe.Status == StatusOsModel.Rejeitada) && SaldoRestante <= 0;
    public bool PodeRegistrarPagamento => Detalhe is not null && Detalhe.Status != StatusOsModel.Entregue && SaldoRestante > 0;

    public decimal SubtotalServicos => Detalhe?.Servicos.Sum(s => s.Subtotal) ?? 0m;
    public decimal SubtotalProdutos => Detalhe?.Produtos.Sum(p => p.Subtotal) ?? 0m;
    public decimal SubtotalTaxas => Detalhe?.Taxas.Sum(t => t.Valor) ?? 0m;
    public decimal SubtotalBruto => SubtotalServicos + SubtotalProdutos + SubtotalTaxas;
    public decimal DescontoAplicado => Detalhe?.ValorDesconto ?? 0m;
    public decimal TotalFinal => Detalhe?.ValorTotal ?? 0m;
    public decimal ValorPago => Detalhe?.Pagamentos.Sum(p => p.Valor) ?? 0m;
    public decimal SaldoRestante => Math.Max(TotalFinal - ValorPago, 0m);
    public IReadOnlyList<MeioPagamentoModel> MeiosPagamentoDisponiveis { get; } = Enum.GetValues<MeioPagamentoModel>();

    public async Task<OperationResult> CarregarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        OrdemServicoId = id;
        SetLoadingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var response = await _ordensServicoApi.ObterPorIdAsync(id, cancellationToken);
        if (!response.Succeeded || response.Data is null)
        {
            var message = response.Error?.Message ?? "Nao foi possivel carregar o detalhe da OS.";
            SetErrorState(message);
            return OperationResult.Failure(message, response.Error?.ValidationErrors);
        }

        var os = response.Data.Deserialize<OrdemServicoResponseModel>(SerializerOptions);
        if (os is null)
        {
            const string message = "Falha ao desserializar o detalhe da OS.";
            SetErrorState(message);
            return OperationResult.Failure(message);
        }

        Detalhe = os;
        _ordemServicoFlowState.SetUltimaOs(os);
        SetSuccessState();
        return OperationResult.Success();
    }

    public async Task<OperationResult> AdicionarServicoAsync(CancellationToken cancellationToken = default)
    {
        var lockResult = ValidarEdicaoPermitida();
        if (lockResult is not null)
        {
            return lockResult;
        }

        if (!TryGetOrdemServicoId(out var osId, out var idError))
        {
            SetErrorState(idError!);
            return OperationResult.Failure(idError!);
        }

        var localErrors = ValidarItem(ServicoForm, "servico");
        if (localErrors.Count > 0)
        {
            ValidationErrors = localErrors;
            SetErrorState("Corrija os campos de servico antes de continuar.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var request = new AdicionarServicoRequestModel(
            Descricao: ServicoForm.Descricao.Trim(),
            Quantidade: ServicoForm.Quantidade,
            ValorUnitario: ServicoForm.ValorUnitario);

        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _ordensServicoApi.AdicionarServicoAsync(osId, payload, cancellationToken);
        if (!result.Succeeded)
        {
            var message = result.Error?.Message ?? "Falha ao adicionar servico.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        ServicoForm = new OrdemServicoComposicaoItemFormModel();
        return await RecarregarAposOperacaoAsync(osId, "Servico adicionado com sucesso.", cancellationToken);
    }

    public async Task<OperationResult> AdicionarProdutoAsync(CancellationToken cancellationToken = default)
    {
        var lockResult = ValidarEdicaoPermitida();
        if (lockResult is not null)
        {
            return lockResult;
        }

        if (!TryGetOrdemServicoId(out var osId, out var idError))
        {
            SetErrorState(idError!);
            return OperationResult.Failure(idError!);
        }

        var localErrors = ValidarItem(ProdutoForm, "produto");
        if (localErrors.Count > 0)
        {
            ValidationErrors = localErrors;
            SetErrorState("Corrija os campos de produto antes de continuar.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var request = new AdicionarProdutoRequestModel(
            Descricao: ProdutoForm.Descricao.Trim(),
            Quantidade: ProdutoForm.Quantidade,
            ValorUnitario: ProdutoForm.ValorUnitario);

        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _ordensServicoApi.AdicionarProdutoAsync(osId, payload, cancellationToken);
        if (!result.Succeeded)
        {
            var message = result.Error?.Message ?? "Falha ao adicionar produto.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        ProdutoForm = new OrdemServicoComposicaoItemFormModel();
        return await RecarregarAposOperacaoAsync(osId, "Produto adicionado com sucesso.", cancellationToken);
    }

    public async Task<OperationResult> AdicionarTaxaAsync(CancellationToken cancellationToken = default)
    {
        var lockResult = ValidarEdicaoPermitida();
        if (lockResult is not null)
        {
            return lockResult;
        }

        if (!TryGetOrdemServicoId(out var osId, out var idError))
        {
            SetErrorState(idError!);
            return OperationResult.Failure(idError!);
        }

        var localErrors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(TaxaForm.Descricao))
        {
            localErrors["Taxa.Descricao"] = ["Descricao da taxa e obrigatoria."];
        }

        if (TaxaForm.Valor <= 0)
        {
            localErrors["Taxa.Valor"] = ["Valor da taxa deve ser maior que zero."];
        }

        if (localErrors.Count > 0)
        {
            ValidationErrors = localErrors;
            SetErrorState("Corrija os campos da taxa antes de continuar.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var request = new AdicionarTaxaRequestModel(
            Descricao: TaxaForm.Descricao.Trim(),
            Valor: TaxaForm.Valor);

        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _ordensServicoApi.AdicionarTaxaAsync(osId, payload, cancellationToken);
        if (!result.Succeeded)
        {
            var message = result.Error?.Message ?? "Falha ao adicionar taxa.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        TaxaForm = new OrdemServicoTaxaFormModel();
        return await RecarregarAposOperacaoAsync(osId, "Taxa adicionada com sucesso.", cancellationToken);
    }

    public async Task<OperationResult> AplicarDescontoAsync(CancellationToken cancellationToken = default)
    {
        var lockResult = ValidarEdicaoPermitida();
        if (lockResult is not null)
        {
            return lockResult;
        }

        if (!TryGetOrdemServicoId(out var osId, out var idError))
        {
            SetErrorState(idError!);
            return OperationResult.Failure(idError!);
        }

        if (DescontoForm.Valor <= 0)
        {
            ValidationErrors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Desconto.Valor"] = ["Valor do desconto deve ser maior que zero."]
            };

            SetErrorState("Corrija os campos de desconto antes de continuar.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var request = new AplicarDescontoRequestModel(
            Tipo: DescontoForm.Tipo,
            Valor: DescontoForm.Valor);

        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _ordensServicoApi.AplicarDescontoAsync(osId, payload, cancellationToken);
        if (!result.Succeeded)
        {
            var message = result.Error?.Message ?? "Falha ao aplicar desconto.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        return await RecarregarAposOperacaoAsync(osId, "Desconto aplicado com sucesso.", cancellationToken);
    }

    public async Task<OperationResult> AdicionarAnotacaoAsync(CancellationToken cancellationToken = default)
    {
        var lockResult = ValidarEdicaoPermitida();
        if (lockResult is not null)
        {
            return lockResult;
        }

        if (!TryGetOrdemServicoId(out var osId, out var idError))
        {
            SetErrorState(idError!);
            return OperationResult.Failure(idError!);
        }

        var localErrors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(AnotacaoForm.Texto))
        {
            localErrors["Anotacao.Texto"] = ["Texto da anotacao e obrigatorio."];
        }

        if (string.IsNullOrWhiteSpace(AnotacaoForm.Autor))
        {
            localErrors["Anotacao.Autor"] = ["Autor da anotacao e obrigatorio."];
        }

        if (localErrors.Count > 0)
        {
            ValidationErrors = localErrors;
            SetErrorState("Corrija os campos da anotacao antes de continuar.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var request = new AdicionarAnotacaoRequestModel(
            Texto: AnotacaoForm.Texto.Trim(),
            Autor: AnotacaoForm.Autor.Trim());

        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _ordensServicoApi.AdicionarAnotacaoAsync(osId, payload, cancellationToken);
        if (!result.Succeeded)
        {
            var message = result.Error?.Message ?? "Falha ao adicionar anotacao.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        AnotacaoForm = new OrdemServicoAnotacaoFormModel();
        return await RecarregarAposOperacaoAsync(osId, "Anotacao adicionada com sucesso.", cancellationToken);
    }

    public async Task<OperationResult> EnviarOrcamentoAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetOrdemServicoId(out var osId, out var idError))
        {
            SetErrorState(idError!);
            return OperationResult.Failure(idError!);
        }

        if (!PodeEnviarOrcamento)
        {
            const string message = "Apenas OS em Rascunho podem ser enviadas como Orcamento.";
            SetErrorState(message);
            return OperationResult.Failure(message);
        }

        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var request = new AlterarStatusRequestModel(StatusOsModel.Orcamento);
        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();

        var result = await _ordensServicoApi.AlterarStatusAsync(osId, payload, cancellationToken);
        if (!result.Succeeded)
        {
            var message = result.Error?.Message ?? "Falha ao enviar OS como Orcamento.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        return await RecarregarAposOperacaoAsync(osId, "OS enviada como Orcamento com sucesso.", cancellationToken);
    }

    public async Task<OperationResult> RegistrarPagamentoAsync(CancellationToken cancellationToken = default)
    {
        if (!TryGetOrdemServicoId(out var osId, out var idError))
        {
            SetErrorState(idError!);
            return OperationResult.Failure(idError!);
        }

        if (!PodeRegistrarPagamento)
        {
            const string message = "Nao ha pagamentos pendentes para registrar com o status atual.";
            SetErrorState(message);
            return OperationResult.Failure(message);
        }

        if (PagamentoForm.Valor <= 0)
        {
            ValidationErrors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Pagamento.Valor"] = ["Valor do pagamento deve ser maior que zero."]
            };

            SetErrorState("Corrija os campos de pagamento antes de continuar.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var request = new RegistrarPagamentoRequestModel(
            Meio: PagamentoForm.Meio,
            Valor: PagamentoForm.Valor,
            DataPagamento: PagamentoForm.DataPagamento,
            ExpectedUpdatedAt: Detalhe?.UpdatedAt);

        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _ordensServicoApi.RegistrarPagamentoAsync(osId, payload, cancellationToken);
        if (!result.Succeeded)
        {
            var message = result.Error?.Message ?? "Falha ao registrar pagamento.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        PagamentoForm = new OrdemServicoPagamentoFormModel();
        return await RecarregarAposOperacaoAsync(osId, "Pagamento registrado com sucesso.", cancellationToken);
    }

    public async Task<OperationResult> AprovarAsync(CancellationToken cancellationToken = default)
        => await AlterarStatusAsync(StatusOsModel.Aprovada, "OS aprovada.", PodeAprovar, cancellationToken);

    public async Task<OperationResult> RejeitarAsync(CancellationToken cancellationToken = default)
        => await AlterarStatusAsync(StatusOsModel.Rejeitada, "OS rejeitada.", PodeRejeitar, cancellationToken);

    public async Task<OperationResult> IniciarAndamentoAsync(CancellationToken cancellationToken = default)
        => await AlterarStatusAsync(StatusOsModel.EmAndamento, "OS em andamento.", PodeIniciarAndamento, cancellationToken);

    public async Task<OperationResult> ConcluirAsync(CancellationToken cancellationToken = default)
        => await AlterarStatusAsync(StatusOsModel.Concluida, "OS concluida.", PodeConcluir, cancellationToken);

    public async Task<OperationResult> EntregarAsync(CancellationToken cancellationToken = default)
        => await AlterarStatusAsync(StatusOsModel.Entregue, "OS entregue.", PodeEntregar, cancellationToken);

    private async Task<OperationResult> RecarregarAposOperacaoAsync(Guid osId, string successMessage, CancellationToken cancellationToken)
    {
        var reload = await CarregarAsync(osId, cancellationToken);
        if (!reload.Succeeded)
        {
            return reload;
        }

        SetSuccessState(successMessage);
        return OperationResult.Success(successMessage);
    }

    private bool TryGetOrdemServicoId(out Guid id, out string? error)
    {
        if (OrdemServicoId is Guid current)
        {
            id = current;
            error = null;
            return true;
        }

        id = Guid.Empty;
        error = "OS nao informada para operacao.";
        return false;
    }

    private static Dictionary<string, string[]> ValidarItem(OrdemServicoComposicaoItemFormModel form, string prefix)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(form.Descricao))
        {
            errors[$"{prefix}.Descricao"] = ["Descricao e obrigatoria."];
        }

        if (form.Quantidade <= 0)
        {
            errors[$"{prefix}.Quantidade"] = ["Quantidade deve ser maior que zero."];
        }

        if (form.ValorUnitario <= 0)
        {
            errors[$"{prefix}.ValorUnitario"] = ["Valor unitario deve ser maior que zero."];
        }

        return errors;
    }

    private OperationResult? ValidarEdicaoPermitida()
    {
        if (!IsEdicaoBloqueadaAposEnvio)
        {
            return null;
        }

        const string message = "Edicao bloqueada apos envio para Orcamento.";
        SetErrorState(message);
        return OperationResult.Failure(message);
    }

    private async Task<OperationResult> AlterarStatusAsync(
        StatusOsModel novoStatus,
        string successMessage,
        bool permitido,
        CancellationToken cancellationToken)
    {
        if (!TryGetOrdemServicoId(out var osId, out var idError))
        {
            SetErrorState(idError!);
            return OperationResult.Failure(idError!);
        }

        if (!permitido)
        {
            const string message = "Transicao de status nao permitida no estado atual.";
            SetErrorState(message);
            return OperationResult.Failure(message);
        }

        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var request = new AlterarStatusRequestModel(novoStatus, Detalhe?.UpdatedAt);
        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();

        var result = await _ordensServicoApi.AlterarStatusAsync(osId, payload, cancellationToken);
        if (!result.Succeeded)
        {
            var message = result.Error?.Message ?? "Falha ao alterar status da OS.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        return await RecarregarAposOperacaoAsync(osId, successMessage, cancellationToken);
    }
}
