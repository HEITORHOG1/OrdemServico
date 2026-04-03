using System.Text.Json;
using System.Text.Json.Nodes;
using Web.Models.Requests;
using Web.Models.Responses;
using Web.Services.Auth;
using Web.State;
using Web.ViewModels.Foundation;

namespace Web.ViewModels.Auth;

public sealed class LoginViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IAuthApi _authApi;
    private readonly TokenStorage _tokenStorage;
    private readonly AuthStateProvider _authStateProvider;

    private LoginFormModel _form = new();

    public LoginViewModel(IAuthApi authApi, TokenStorage tokenStorage, AuthStateProvider authStateProvider)
    {
        _authApi = authApi;
        _tokenStorage = tokenStorage;
        _authStateProvider = authStateProvider;
    }

    public LoginFormModel Form
    {
        get => _form;
        private set => SetProperty(ref _form, value);
    }

    public async Task<OperationResult> LoginAsync(CancellationToken cancellationToken = default)
    {
        SetSubmittingState();

        var localValidation = ValidarFormulario();
        if (localValidation.Count > 0)
        {
            SetErrorState("Preencha todos os campos.");
            return OperationResult.Failure(ErrorMessage!, localValidation);
        }

        var request = new LoginRequestModel(Form.Email.Trim(), Form.Senha);
        var payload = JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
        var result = await _authApi.LoginAsync(payload, cancellationToken);

        if (!result.Succeeded || result.Data is null)
        {
            var message = result.Error?.Message ?? "Email ou senha invalidos.";
            SetErrorState(message);
            return OperationResult.Failure(message);
        }

        var loginResponse = result.Data.Deserialize<LoginResponseModel>(SerializerOptions);
        if (loginResponse is null)
        {
            SetErrorState("Falha ao processar resposta do login.");
            return OperationResult.Failure(ErrorMessage!);
        }

        await _tokenStorage.SalvarTokensAsync(loginResponse.AccessToken, loginResponse.RefreshToken);
        _authStateProvider.NotificarLogin();

        SetSuccessState("Login realizado com sucesso.");
        Form = new LoginFormModel();
        return OperationResult.Success(SuccessMessage);
    }

    public async Task LogoutAsync()
    {
        await _authStateProvider.NotificarLogoutAsync();
    }

    private Dictionary<string, string[]> ValidarFormulario()
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(Form.Email))
            errors["Email"] = ["Email e obrigatorio."];
        else if (!Form.Email.Contains('@'))
            errors["Email"] = ["Email com formato invalido."];

        if (string.IsNullOrWhiteSpace(Form.Senha))
            errors["Senha"] = ["Senha e obrigatoria."];

        return errors;
    }
}
