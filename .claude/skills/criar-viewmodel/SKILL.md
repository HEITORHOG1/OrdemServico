---
name: criar-viewmodel
description: Cria um ViewModel Blazor MVVM completo seguindo o padrao do projeto - com ViewModelBase, state management, form model e API integration.
disable-model-invocation: true
user-invocable: true
argument-hint: "[nome-do-viewmodel]"
---

# Criar ViewModel Blazor MVVM — $ARGUMENTS

Crie o ViewModel **$ARGUMENTS** em `src/Web/ViewModels/` seguindo o padrao MVVM do projeto.

## 1. Form Model (se for tela de cadastro/edicao)

Criar em `ViewModels/{Feature}/{Feature}FormModel.cs`:

```csharp
namespace Web.ViewModels.{Feature};

public sealed class {Feature}FormModel
{
    public string Campo { get; set; } = string.Empty;
    public string? CampoOpcional { get; set; }
    public DateTime? DataOpcional { get; set; }
}
```

## 2. ViewModel

Criar em `ViewModels/{Feature}/{Feature}ViewModel.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Nodes;
using Web.Services.Api;
using Web.State;
using Web.ViewModels.Foundation;

namespace Web.ViewModels.{Feature};

public sealed class {Feature}ViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly I{Feature}Api _api;
    private {Feature}FormModel _form = new();
    private IReadOnlyDictionary<string, string[]> _validationErrors = new Dictionary<string, string[]>();

    public {Feature}ViewModel(I{Feature}Api api)
    {
        _api = api;
    }

    public {Feature}FormModel Form
    {
        get => _form;
        private set => SetProperty(ref _form, value);
    }

    public IReadOnlyDictionary<string, string[]> ValidationErrors
    {
        get => _validationErrors;
        private set => SetProperty(ref _validationErrors, value);
    }

    public async Task<OperationResult> CarregarAsync(CancellationToken ct = default)
    {
        SetLoadingState();

        var result = await _api.ListarAsync(ct);
        if (!result.Succeeded || result.Data is null)
        {
            var message = result.Error?.Message ?? "Falha ao carregar dados.";
            SetErrorState(message);
            return OperationResult.Failure(message);
        }

        // Deserializar e popular propriedades...
        SetSuccessState();
        return OperationResult.Success(SuccessMessage);
    }

    public async Task<OperationResult> SubmitAsync(CancellationToken ct = default)
    {
        SetSubmittingState();
        ValidationErrors = new Dictionary<string, string[]>();

        // 1. Validacao local
        var localErrors = ValidarFormulario();
        if (localErrors.Count > 0)
        {
            ValidationErrors = localErrors;
            SetErrorState("Corrija os campos obrigatorios.");
            return OperationResult.Failure(ErrorMessage!, ValidationErrors);
        }

        // 2. Chamar API
        var payload = CriarPayload();
        var result = await _api.CriarAsync(payload, ct);

        if (!result.Succeeded || result.Data is null)
        {
            var message = result.Error?.Message ?? "Falha ao salvar.";
            ValidationErrors = result.Error?.ValidationErrors ?? new Dictionary<string, string[]>();
            SetErrorState(message);
            return OperationResult.Failure(message, ValidationErrors);
        }

        // 3. Processar resposta
        SetSuccessState("Salvo com sucesso.");
        return OperationResult.Success(SuccessMessage);
    }

    private Dictionary<string, string[]> ValidarFormulario()
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(Form.Campo))
            errors["Campo"] = ["Campo e obrigatorio."];

        return errors;
    }

    private JsonObject CriarPayload()
    {
        var request = new { /* montar request a partir do Form */ };
        return JsonSerializer.SerializeToNode(request, SerializerOptions) as JsonObject ?? new JsonObject();
    }

    private static string? NormalizarCampo(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
```

## 3. Registrar no DI

Em `src/Web/Program.cs` ou `DependencyInjection`:
```csharp
services.AddTransient<{Feature}ViewModel>();
```

## Regras

- **Sempre herdar de `ViewModelBase`**
- **`SetProperty()`** para todas as propriedades observaveis
- **State management**: usar `SetLoadingState()`, `SetSubmittingState()`, `SetErrorState()`, `SetSuccessState()`
- **`OperationResult`** como retorno de toda operacao publica
- **Validacao local ANTES da chamada API**
- **`JsonSerializerDefaults.Web`** para deserializacao
- **Sempre verificar `result.Succeeded`** antes de acessar `.Data`
- **`NormalizarCampo()`** para limpar inputs (trim + empty->null)
- **Flow states** para compartilhar dados entre telas quando necessario
