using System.Text.Json;
using System.Text.Json.Nodes;
using Web.Models.Requests;
using Web.Models.Responses;
using Web.Services.Api;
using Web.State;
using Web.ViewModels.Foundation;

namespace Web.ViewModels.OrdensServico;

public sealed class OrdemServicoCadastroViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IOrdensServicoApi _ordensServicoApi;
    private readonly IClientesApi _clientesApi;
    private readonly IEquipamentosApi _equipamentosApi;
    private readonly ClienteFlowState _clienteFlowState;
    private readonly EquipamentoFlowState _equipamentoFlowState;
    private readonly OrdemServicoFlowState _ordemServicoFlowState;

    private OrdemServicoCadastroFormModel _form = new();
    private IReadOnlyDictionary<string, string[]> _validationErrors = new Dictionary<string, string[]>();
    private OrdemServicoResponseModel? _ultimaCriada;
    private string _buscaClienteNome = string.Empty;

    public OrdemServicoCadastroViewModel(
        IOrdensServicoApi ordensServicoApi,
        IClientesApi clientesApi,
        IEquipamentosApi equipamentosApi,
        ClienteFlowState clienteFlowState,
        EquipamentoFlowState equipamentoFlowState,
        OrdemServicoFlowState ordemServicoFlowState)
    {
        _ordensServicoApi = ordensServicoApi;
        _clientesApi = clientesApi;
        _equipamentosApi = equipamentosApi;
        _clienteFlowState = clienteFlowState;
        _equipamentoFlowState = equipamentoFlowState;
        _ordemServicoFlowState = ordemServicoFlowState;

        PreencherContextoInicial();
    }

    public OrdemServicoCadastroFormModel Form
    {
        get => _form;
        private set => SetProperty(ref _form, value);
    }

    public IReadOnlyDictionary<string, string[]> ValidationErrors
    {
        get => _validationErrors;
        private set => SetProperty(ref _validationErrors, value);
    }

    public OrdemServicoResponseModel? UltimaCriada
    {
        get => _ultimaCriada;
        private set => SetProperty(ref _ultimaCriada, value);
    }

    public string BuscaClienteNome
    {
        get => _buscaClienteNome;
        set => SetProperty(ref _buscaClienteNome, value);
    }

    public List<ClienteResponseModel> ClientesBusca { get; } = new();
    public List<EquipamentoResponseModel> EquipamentosBusca { get; } = new();
    public ClienteResponseModel? ClienteSelecionado { get; private set; }
    public EquipamentoResponseModel? EquipamentoSelecionado { get; private set; }

    public void PreencherContextoInicial()
    {
        if (_clienteFlowState.UltimoClienteId is Guid clienteId)
        {
            Form.ClienteId = clienteId.ToString();
        }

        if (_equipamentoFlowState.EquipamentoSelecionadoId is Guid equipamentoId)
        {
            Form.EquipamentoId = equipamentoId.ToString();
        }
    }

    public async Task<OperationResult> BuscarClientesPorNomeAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(BuscaClienteNome) || BuscaClienteNome.Trim().Length < 2)
        {
            SetErrorState("Informe pelo menos 2 caracteres para buscar.");
            return OperationResult.Failure(ErrorMessage!);
        }

        SetLoadingState();
        ClientesBusca.Clear();

        var result = await _clientesApi.BuscarPorNomeAsync(BuscaClienteNome.Trim(), cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            var message = result.Error?.Message ?? "Falha ao buscar clientes.";
            SetErrorState(message);
            return OperationResult.Failure(message);
        }

        foreach (var node in result.Data)
        {
            if (node is not JsonObject obj) continue;
            var cliente = obj.Deserialize<ClienteResponseModel>(SerializerOptions);
            if (cliente is not null)
            {
                ClientesBusca.Add(cliente);
            }
        }

        SetSuccessState($"{ClientesBusca.Count} cliente(s) encontrado(s).");
        return OperationResult.Success(SuccessMessage);
    }

    public async Task<OperationResult> SelecionarClienteAsync(ClienteResponseModel cliente, CancellationToken cancellationToken = default)
    {
        ClienteSelecionado = cliente;
        Form.ClienteId = cliente.Id.ToString();
        _clienteFlowState.SetUltimoCliente(cliente);

        // Carregar equipamentos do cliente
        EquipamentosBusca.Clear();
        EquipamentoSelecionado = null;
        Form.EquipamentoId = null;

        var result = await _equipamentosApi.ListarPorClienteAsync(cliente.Id, cancellationToken);
        if (result.Succeeded && result.Data is not null)
        {
            foreach (var node in result.Data)
            {
                if (node is not JsonObject obj) continue;
                var equipamento = obj.Deserialize<EquipamentoResponseModel>(SerializerOptions);
                if (equipamento is not null)
                {
                    EquipamentosBusca.Add(equipamento);
                }
            }
        }

        SetSuccessState($"Cliente {cliente.Nome} selecionado. {EquipamentosBusca.Count} equipamento(s) encontrado(s).");
        return OperationResult.Success(SuccessMessage);
    }

    public void SelecionarEquipamento(EquipamentoResponseModel equipamento)
    {
        EquipamentoSelecionado = equipamento;
        Form.EquipamentoId = equipamento.Id.ToString();
    }

    public async Task<OperationResult> SubmitAsync(CancellationToken cancellationToken = default)
    {
        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        var localValidation = ValidarFormulario();
        if (localValidation.Count > 0)
        {
            ValidationErrors = localValidation;
            SetErrorState("Corrija os campos obrigatorios antes de enviar.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        var request = CriarRequest();
        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();

        var result = await _ordensServicoApi.CriarAsync(payload, cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            var message = result.Error?.Message ?? "Falha ao criar Ordem de Servico.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        var os = result.Data.Deserialize<OrdemServicoResponseModel>(SerializerOptions);
        if (os is null)
        {
            SetErrorState("OS criada, mas houve falha ao processar a resposta da API.");
            return OperationResult.Failure(ErrorMessage!);
        }

        UltimaCriada = os;
        _ordemServicoFlowState.SetUltimaOs(os);
        SetSuccessState("OS criada com sucesso.");

        return OperationResult.Success(SuccessMessage);
    }

    public async Task<ApiResult<OrdemServicoResponseModel>> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        SetLoadingState();
        var result = await _ordensServicoApi.ObterPorIdAsync(id, cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            var message = result.Error?.Message ?? "Nao foi possivel obter a OS informada.";
            SetErrorState(message);
            return new ApiResult<OrdemServicoResponseModel>(false, result.StatusCode, null, result.Error);
        }

        var os = result.Data.Deserialize<OrdemServicoResponseModel>(SerializerOptions);
        if (os is null)
        {
            SetErrorState("Resposta da API invalida para detalhe da OS.");
            return new ApiResult<OrdemServicoResponseModel>(false, result.StatusCode, null, new ApiError(ErrorMessage!));
        }

        SetSuccessState();
        return new ApiResult<OrdemServicoResponseModel>(true, result.StatusCode, os, null);
    }

    private CriarOrdemServicoRequestModel CriarRequest()
    {
        var clienteId = Guid.Parse(Form.ClienteId);
        Guid? equipamentoId = null;
        if (Guid.TryParse(Form.EquipamentoId, out var parsedEquipamentoId))
        {
            equipamentoId = parsedEquipamentoId;
        }

        return new CriarOrdemServicoRequestModel(
            ClienteId: clienteId,
            EquipamentoId: equipamentoId,
            Defeito: Form.Defeito.Trim(),
            Duracao: NormalizarCampo(Form.Duracao),
            Observacoes: NormalizarCampo(Form.Observacoes),
            Referencia: NormalizarCampo(Form.Referencia),
            ValidadeOrcamento: Form.ValidadeOrcamento is null ? null : DateOnly.FromDateTime(Form.ValidadeOrcamento.Value),
            PrazoEntrega: Form.PrazoEntrega is null ? null : DateOnly.FromDateTime(Form.PrazoEntrega.Value));
    }

    private Dictionary<string, string[]> ValidarFormulario()
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (!Guid.TryParse(Form.ClienteId, out _))
        {
            errors["ClienteId"] = ["Selecione um cliente antes de criar a OS."];
        }

        if (!string.IsNullOrWhiteSpace(Form.EquipamentoId) && !Guid.TryParse(Form.EquipamentoId, out _))
        {
            errors["EquipamentoId"] = ["EquipamentoId invalido."];
        }

        if (string.IsNullOrWhiteSpace(Form.Defeito))
        {
            errors["Defeito"] = ["Defeito e obrigatorio."];
        }

        return errors;
    }

    private static string? NormalizarCampo(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
