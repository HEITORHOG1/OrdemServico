using System.Text.Json;
using System.Text.Json.Nodes;
using Web.Models.Requests;
using Web.Models.Responses;
using Web.Services.Api;
using Web.State;
using Web.ViewModels.Foundation;

namespace Web.ViewModels.Clientes;

public sealed class ClienteCadastroViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IClientesApi _clientesApi;
    private readonly ClienteFlowState _clienteFlowState;

    private ClienteCadastroFormModel _form = new();
    private bool _formVisivel;
    private string _buscaNome = string.Empty;
    private IReadOnlyDictionary<string, string[]> _validationErrors = new Dictionary<string, string[]>();

    public ClienteCadastroViewModel(IClientesApi clientesApi, ClienteFlowState clienteFlowState)
    {
        _clientesApi = clientesApi;
        _clienteFlowState = clienteFlowState;
    }

    public ClienteCadastroFormModel Form
    {
        get => _form;
        private set => SetProperty(ref _form, value);
    }

    public bool FormVisivel
    {
        get => _formVisivel;
        private set => SetProperty(ref _formVisivel, value);
    }

    public string BuscaNome
    {
        get => _buscaNome;
        set => SetProperty(ref _buscaNome, value);
    }

    public IReadOnlyDictionary<string, string[]> ValidationErrors
    {
        get => _validationErrors;
        private set => SetProperty(ref _validationErrors, value);
    }

    public List<ClienteResponseModel> ClientesCriados { get; } = new();
    public List<ClienteResponseModel> ClientesBusca { get; } = new();

    public Guid? UltimoClienteId => _clienteFlowState.UltimoClienteId;
    public string? UltimoClienteNome => _clienteFlowState.UltimoClienteNome;

    public void AbrirNovoCliente()
    {
        FormVisivel = true;
        ErrorMessage = null;
        SuccessMessage = null;
        ValidationErrors = new Dictionary<string, string[]>();
    }

    public void CancelarCadastro()
    {
        FormVisivel = false;
        Form = new ClienteCadastroFormModel();
        ValidationErrors = new Dictionary<string, string[]>();
        ErrorMessage = null;
    }

    public async Task<OperationResult> BuscarPorNomeAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(BuscaNome) || BuscaNome.Trim().Length < 2)
        {
            SetErrorState("Informe pelo menos 2 caracteres para buscar.");
            return OperationResult.Failure(ErrorMessage!);
        }

        SetLoadingState();
        ClientesBusca.Clear();

        var result = await _clientesApi.BuscarPorNomeAsync(BuscaNome.Trim(), cancellationToken);
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

    public void SelecionarCliente(ClienteResponseModel cliente)
    {
        _clienteFlowState.SetUltimoCliente(cliente);
        SetSuccessState($"Cliente {cliente.Nome} selecionado.");
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

        var request = new CriarClienteRequestModel(
            Nome: Form.Nome.Trim(),
            Documento: NormalizarCampo(Form.Documento),
            Telefone: NormalizarCampo(Form.Telefone),
            Email: NormalizarCampo(Form.Email),
            Endereco: NormalizarCampo(Form.Endereco));

        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _clientesApi.CriarAsync(payload, cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            var errorMessage = result.Error?.Message ?? "Falha ao cadastrar cliente.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(errorMessage);
            return OperationResult.Failure(errorMessage, ValidationErrors);
        }

        var cliente = result.Data.Deserialize<ClienteResponseModel>(SerializerOptions);
        if (cliente is null)
        {
            SetErrorState("Cliente criado, mas houve falha ao processar a resposta da API.");
            return OperationResult.Failure(ErrorMessage!);
        }

        ClientesCriados.Insert(0, cliente);
        _clienteFlowState.SetUltimoCliente(cliente);

        SetSuccessState("Cliente cadastrado com sucesso.");
        Form = new ClienteCadastroFormModel();
        FormVisivel = false;

        return OperationResult.Success(SuccessMessage);
    }

    private Dictionary<string, string[]> ValidarFormulario()
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(Form.Nome))
        {
            errors["Nome"] = ["Nome e obrigatorio."];
        }

        if (!string.IsNullOrWhiteSpace(Form.Email) && !Form.Email.Contains('@'))
        {
            errors["Email"] = ["E-mail com formato invalido."];
        }

        return errors;
    }

    private static string? NormalizarCampo(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
