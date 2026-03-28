using System.Text.Json;
using System.Text.Json.Nodes;
using Web.Models.Requests;
using Web.Models.Responses;
using Web.Services.Api;
using Web.State;
using Web.ViewModels.Foundation;

namespace Web.ViewModels.Equipamentos;

public sealed class EquipamentosViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IEquipamentosApi _equipamentosApi;
    private readonly IClientesApi _clientesApi;
    private readonly ClienteFlowState _clienteFlowState;
    private readonly EquipamentoFlowState _equipamentoFlowState;

    private EquipamentoCadastroFormModel _form = new();
    private bool _formVisivel;
    private string _buscaClienteNome = string.Empty;
    private IReadOnlyDictionary<string, string[]> _validationErrors = new Dictionary<string, string[]>();

    public EquipamentosViewModel(
        IEquipamentosApi equipamentosApi,
        IClientesApi clientesApi,
        ClienteFlowState clienteFlowState,
        EquipamentoFlowState equipamentoFlowState)
    {
        _equipamentosApi = equipamentosApi;
        _clientesApi = clientesApi;
        _clienteFlowState = clienteFlowState;
        _equipamentoFlowState = equipamentoFlowState;
    }

    public EquipamentoCadastroFormModel Form
    {
        get => _form;
        private set => SetProperty(ref _form, value);
    }

    public bool FormVisivel
    {
        get => _formVisivel;
        private set => SetProperty(ref _formVisivel, value);
    }

    public string BuscaClienteNome
    {
        get => _buscaClienteNome;
        set => SetProperty(ref _buscaClienteNome, value);
    }

    public IReadOnlyDictionary<string, string[]> ValidationErrors
    {
        get => _validationErrors;
        private set => SetProperty(ref _validationErrors, value);
    }

    public List<ClienteResponseModel> ClientesBusca { get; } = new();
    public List<EquipamentoResponseModel> Equipamentos { get; } = new();

    public ClienteResponseModel? ClienteSelecionado { get; private set; }
    public Guid? ClienteIdContexto => _equipamentoFlowState.ClienteIdContexto;
    public Guid? EquipamentoSelecionadoId => _equipamentoFlowState.EquipamentoSelecionadoId;
    public string? EquipamentoSelecionadoDescricao => _equipamentoFlowState.EquipamentoSelecionadoDescricao;

    public void AbrirNovoEquipamento()
    {
        FormVisivel = true;
        ErrorMessage = null;
        SuccessMessage = null;
        ValidationErrors = new Dictionary<string, string[]>();
    }

    public void CancelarCadastro()
    {
        FormVisivel = false;
        Form = new EquipamentoCadastroFormModel();
        ValidationErrors = new Dictionary<string, string[]>();
        ErrorMessage = null;
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

    public async Task<OperationResult> SelecionarClienteECarregarEquipamentosAsync(ClienteResponseModel cliente, CancellationToken cancellationToken = default)
    {
        ClienteSelecionado = cliente;
        _equipamentoFlowState.DefinirClienteContexto(cliente.Id);
        _clienteFlowState.SetUltimoCliente(cliente);

        return await CarregarEquipamentosDoClienteAsync(cliente.Id, cancellationToken);
    }

    public async Task<OperationResult> CarregarEquipamentosDoClienteAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        SetLoadingState();
        Equipamentos.Clear();

        var result = await _equipamentosApi.ListarPorClienteAsync(clienteId, cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            var message = result.Error?.Message ?? "Falha ao listar equipamentos do cliente.";
            SetErrorState(message);
            return OperationResult.Failure(message, result.Error?.ValidationErrors);
        }

        foreach (var node in result.Data)
        {
            if (node is not JsonObject obj) continue;
            var equipamento = obj.Deserialize<EquipamentoResponseModel>(SerializerOptions);
            if (equipamento is not null)
            {
                Equipamentos.Add(equipamento);
            }
        }

        SetSuccessState($"{Equipamentos.Count} equipamento(s) encontrado(s).");
        return OperationResult.Success(SuccessMessage);
    }

    public async Task<OperationResult> SubmitAsync(CancellationToken cancellationToken = default)
    {
        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        if (ClienteIdContexto is not Guid clienteId)
        {
            SetErrorState("Selecione um cliente antes de cadastrar o equipamento.");
            return OperationResult.Failure(ErrorMessage!);
        }

        var localValidation = ValidarFormulario();
        if (localValidation.Count > 0)
        {
            ValidationErrors = localValidation;
            SetErrorState("Corrija os campos obrigatorios antes de enviar.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        var request = new CriarEquipamentoRequestModel(
            ClienteId: clienteId,
            Tipo: Form.Tipo.Trim(),
            Marca: NormalizarCampo(Form.Marca),
            Modelo: NormalizarCampo(Form.Modelo),
            NumeroSerie: NormalizarCampo(Form.NumeroSerie));

        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _equipamentosApi.CriarAsync(payload, cancellationToken);
        if (!result.Succeeded || result.Data is null)
        {
            var message = result.Error?.Message ?? "Falha ao cadastrar equipamento.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        var equipamento = result.Data.Deserialize<EquipamentoResponseModel>(SerializerOptions);
        if (equipamento is null)
        {
            SetErrorState("Equipamento criado, mas houve falha ao processar a resposta da API.");
            return OperationResult.Failure(ErrorMessage!);
        }

        Equipamentos.Insert(0, equipamento);

        SetSuccessState("Equipamento cadastrado com sucesso.");
        Form = new EquipamentoCadastroFormModel();
        FormVisivel = false;

        return OperationResult.Success(SuccessMessage);
    }

    public OperationResult SelecionarEquipamento(EquipamentoResponseModel equipamento)
    {
        var descricao = string.Join(" - ", new[] { equipamento.Tipo, equipamento.Marca, equipamento.Modelo }.Where(x => !string.IsNullOrWhiteSpace(x)));
        _equipamentoFlowState.SelecionarEquipamento(equipamento.Id, descricao);
        SetSuccessState("Equipamento selecionado para uso na criacao da OS.");
        return OperationResult.Success(SuccessMessage);
    }

    private Dictionary<string, string[]> ValidarFormulario()
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(Form.Tipo))
        {
            errors["Tipo"] = ["Tipo e obrigatorio."];
        }

        return errors;
    }

    private static string? NormalizarCampo(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
