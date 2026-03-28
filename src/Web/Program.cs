using Web.Components;
using Web.Models;
using Web.Models.Requests;
using Web.Models.Responses;
using Web.Services.Api;
using Web.Services.Ui;
using Web.State;
using Web.ViewModels.Clientes;
using Web.ViewModels.Equipamentos;
using Web.ViewModels.OrdensServico;
using Radzen;
using System.Net;
using System.Text.Json.Nodes;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.AddRadzenComponents();
builder.Services.AddHttpClient("OsApi", (serviceProvider, client) =>
{
    var settings = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiSettings>>()
        .Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddScoped<IApiStatusService, ApiStatusService>();
builder.Services.AddScoped<IApiErrorParser, ApiErrorParser>();
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddScoped<IAppNotifier, AppNotifier>();
builder.Services.AddScoped<IClientesApi, ClientesApi>();
builder.Services.AddScoped<IEquipamentosApi, EquipamentosApi>();
builder.Services.AddScoped<IOrdensServicoApi, OrdensServicoApi>();
builder.Services.AddScoped<ClienteFlowState>();
builder.Services.AddScoped<EquipamentoFlowState>();
builder.Services.AddScoped<OrdemServicoFlowState>();
builder.Services.AddScoped<ClienteCadastroViewModel>();
builder.Services.AddScoped<EquipamentosViewModel>();
builder.Services.AddScoped<OrdemServicoCadastroViewModel>();
builder.Services.AddScoped<OrdemServicoDetalheViewModel>();
builder.Services.AddScoped<OrdemServicoListaViewModel>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/internal/api-status", async (IHttpClientFactory httpClientFactory, CancellationToken ct) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("OsApi");
        using var response = await client.GetAsync("/swagger/v1/swagger.json", ct);

        return response.IsSuccessStatusCode
            ? Results.Ok(new { available = true, statusCode = (int)response.StatusCode })
            : Results.Problem($"API indisponivel. Status {(int)response.StatusCode}.");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Falha ao chamar a API: {ex.Message}");
    }
});

app.MapGet("/internal/api-services-smoke", async (
    IClientesApi clientesApi,
    IEquipamentosApi equipamentosApi,
    IOrdensServicoApi ordensServicoApi,
    CancellationToken ct) =>
{
    var clienteRequest = new JsonObject
    {
        ["nome"] = "Cliente Smoke",
        ["documento"] = Guid.NewGuid().ToString("N")[..11],
        ["telefone"] = "11999990000",
        ["email"] = "smoke@os.local",
        ["endereco"] = "Endereco smoke"
    };

    var clienteCreate = await clientesApi.CriarAsync(clienteRequest, ct);
    if (!clienteCreate.Succeeded || clienteCreate.Data is null)
    {
        return Results.Problem($"Falha em ClientesApi.CriarAsync: {clienteCreate.Error?.Message}");
    }

    var clienteId = clienteCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (clienteId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test: id de cliente nao retornado.");
    }

    var clienteGet = await clientesApi.ObterPorIdAsync(clienteId, ct);
    var equipamentos = await equipamentosApi.ListarPorClienteAsync(clienteId, ct);
    var osList = await ordensServicoApi.ListarAsync(1, 10, ct);

    return Results.Ok(new
    {
        clienteCreate = new { clienteCreate.Succeeded, statusCode = (int)clienteCreate.StatusCode },
        clienteGet = new { clienteGet.Succeeded, statusCode = (int)clienteGet.StatusCode },
        equipamentosList = new { equipamentos.Succeeded, statusCode = (int)equipamentos.StatusCode },
        ordensServicoList = new { osList.Succeeded, statusCode = (int)osList.StatusCode }
    });
});

app.MapGet("/internal/models-compatibility-smoke", async (IHttpClientFactory httpClientFactory, CancellationToken ct) =>
{
    var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

    var requestModel = new CriarClienteRequestModel(
        Nome: "Cliente Compat",
        Documento: "12345678900",
        Telefone: "11999990000",
        Email: "compat@os.local",
        Endereco: "Rua Compat, 10");

    var requestJson = JsonSerializer.Serialize(requestModel, serializerOptions);
    using var requestDoc = JsonDocument.Parse(requestJson);
    var hasNome = requestDoc.RootElement.TryGetProperty("nome", out _);
    var hasDocumento = requestDoc.RootElement.TryGetProperty("documento", out _);

    var client = httpClientFactory.CreateClient("OsApi");

    using var clienteCreateResponse = await client.PostAsJsonAsync("/api/clientes", requestModel, serializerOptions, ct);
    if (!clienteCreateResponse.IsSuccessStatusCode)
    {
        return Results.Problem($"Falha ao validar serializacao de request. Status {(int)clienteCreateResponse.StatusCode}.");
    }

    var cliente = await clienteCreateResponse.Content.ReadFromJsonAsync<ClienteResponseModel>(serializerOptions, ct);
    if (cliente is null || cliente.Id == Guid.Empty)
    {
        return Results.Problem("Falha ao desserializar ClienteResponseModel.");
    }

    using var osListResponse = await client.GetAsync("/api/ordens-servico?Page=1&PageSize=5", ct);
    if (!osListResponse.IsSuccessStatusCode)
    {
        return Results.Problem($"Falha ao validar desserializacao da listagem OS. Status {(int)osListResponse.StatusCode}.");
    }

    var osList = await osListResponse.Content.ReadFromJsonAsync<PagedResponseModel<OrdemServicoResumoResponseModel>>(serializerOptions, ct);
    if (osList is null)
    {
        return Results.Problem("Falha ao desserializar PagedResponseModel<OrdemServicoResumoResponseModel>.");
    }

    return Results.Ok(new
    {
        requestSerialization = new { hasNome, hasDocumento },
        clienteResponseDeserialization = new { ok = true, clienteId = cliente.Id },
        ordemServicoListDeserialization = new { ok = true, count = osList.Items.Count }
    });
});

app.MapGet("/internal/clientes-phase4-smoke", async (ClienteCadastroViewModel viewModel, CancellationToken ct) =>
{
    viewModel.AbrirNovoCliente();
    viewModel.Form.Nome = "Cliente Fase 4";
    viewModel.Form.Documento = Guid.NewGuid().ToString("N")[..11];
    viewModel.Form.Telefone = "11988887777";
    viewModel.Form.Email = "fase4@os.local";
    viewModel.Form.Endereco = "Rua Fase 4";

    var result = await viewModel.SubmitAsync(ct);
    if (!result.Succeeded)
    {
        return Results.Problem(result.Message ?? "Falha ao executar fluxo de cadastro de cliente da fase 4.");
    }

    return Results.Ok(new
    {
        result.Succeeded,
        result.Message,
        ultimoClienteId = viewModel.UltimoClienteId,
        clientesCriados = viewModel.ClientesCriados.Count
    });
});

app.MapGet("/internal/equipamentos-phase5-smoke", async (
    IClientesApi clientesApi,
    EquipamentosViewModel viewModel,
    CancellationToken ct) =>
{
    var clienteRequest = new JsonObject
    {
        ["nome"] = "Cliente Fase 5",
        ["documento"] = Guid.NewGuid().ToString("N")[..11],
        ["telefone"] = "11977776666",
        ["email"] = "fase5@os.local",
        ["endereco"] = "Rua Fase 5"
    };

    var clienteCreate = await clientesApi.CriarAsync(clienteRequest, ct);
    if (!clienteCreate.Succeeded || clienteCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar cliente para fase 5: {clienteCreate.Error?.Message}");
    }

    var clienteId = clienteCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (clienteId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 5: clienteId nao retornado.");
    }

    var clienteModel = new ClienteResponseModel(clienteId, "Cliente Fase 5", null, null, null, null, DateTime.UtcNow, DateTime.UtcNow);
    var listBefore = await viewModel.SelecionarClienteECarregarEquipamentosAsync(clienteModel, ct);
    if (!listBefore.Succeeded)
    {
        return Results.Problem($"Falha ao listar equipamentos: {listBefore.Message}");
    }

    viewModel.AbrirNovoEquipamento();
    viewModel.Form.Tipo = "Notebook";
    viewModel.Form.Marca = "Dell";
    viewModel.Form.Modelo = "Latitude";
    viewModel.Form.NumeroSerie = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();

    var createResult = await viewModel.SubmitAsync(ct);
    if (!createResult.Succeeded || viewModel.Equipamentos.Count == 0)
    {
        return Results.Problem($"Falha ao criar equipamento: {createResult.Message}");
    }

    var selecionado = viewModel.Equipamentos[0];
    var selectResult = viewModel.SelecionarEquipamento(selecionado);
    if (!selectResult.Succeeded)
    {
        return Results.Problem($"Falha ao selecionar equipamento: {selectResult.Message}");
    }

    return Results.Ok(new
    {
        clienteId,
        equipamentosCarregados = viewModel.Equipamentos.Count,
        equipamentoSelecionadoId = viewModel.EquipamentoSelecionadoId,
        equipamentoSelecionadoDescricao = viewModel.EquipamentoSelecionadoDescricao
    });
});

app.MapGet("/internal/ordens-servico-phase6-smoke", async (
    IClientesApi clientesApi,
    IEquipamentosApi equipamentosApi,
    OrdemServicoCadastroViewModel viewModel,
    CancellationToken ct) =>
{
    var clienteRequest = new JsonObject
    {
        ["nome"] = "Cliente Fase 6",
        ["documento"] = Guid.NewGuid().ToString("N")[..11],
        ["telefone"] = "11966665555",
        ["email"] = "fase6@os.local",
        ["endereco"] = "Rua Fase 6"
    };

    var clienteCreate = await clientesApi.CriarAsync(clienteRequest, ct);
    if (!clienteCreate.Succeeded || clienteCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar cliente para fase 6: {clienteCreate.Error?.Message}");
    }

    var clienteId = clienteCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (clienteId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 6: clienteId nao retornado.");
    }

    var equipamentoRequest = new JsonObject
    {
        ["clienteId"] = clienteId,
        ["tipo"] = "Notebook",
        ["marca"] = "Lenovo",
        ["modelo"] = "ThinkPad",
        ["numeroSerie"] = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
    };

    var equipamentoCreate = await equipamentosApi.CriarAsync(equipamentoRequest, ct);
    if (!equipamentoCreate.Succeeded || equipamentoCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar equipamento para fase 6: {equipamentoCreate.Error?.Message}");
    }

    var equipamentoId = equipamentoCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (equipamentoId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 6: equipamentoId nao retornado.");
    }

    viewModel.Form.ClienteId = clienteId.ToString();
    viewModel.Form.EquipamentoId = equipamentoId.ToString();
    viewModel.Form.Defeito = "Nao liga";
    viewModel.Form.Observacoes = "Teste da fase 6";

    var createResult = await viewModel.SubmitAsync(ct);
    if (!createResult.Succeeded || viewModel.UltimaCriada is null)
    {
        return Results.Problem($"Falha ao criar OS na fase 6: {createResult.Message}");
    }

    var osId = viewModel.UltimaCriada.Id;
    var getResult = await viewModel.ObterPorIdAsync(osId, ct);
    if (!getResult.Succeeded || getResult.Data is null)
    {
        return Results.Problem($"Falha ao obter OS criada na fase 6: {getResult.Error?.Message}");
    }

    return Results.Ok(new
    {
        clienteId,
        equipamentoId,
        ordemServicoId = getResult.Data.Id,
        numero = getResult.Data.Numero,
        status = getResult.Data.Status
    });
});

app.MapGet("/internal/ordens-servico-phase7-smoke", async (
    IClientesApi clientesApi,
    IEquipamentosApi equipamentosApi,
    OrdemServicoCadastroViewModel cadastroViewModel,
    OrdemServicoDetalheViewModel detalheViewModel,
    CancellationToken ct) =>
{
    var clienteRequest = new JsonObject
    {
        ["nome"] = "Cliente Fase 7",
        ["documento"] = Guid.NewGuid().ToString("N")[..11],
        ["telefone"] = "11955554444",
        ["email"] = "fase7@os.local",
        ["endereco"] = "Rua Fase 7"
    };

    var clienteCreate = await clientesApi.CriarAsync(clienteRequest, ct);
    if (!clienteCreate.Succeeded || clienteCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar cliente para fase 7: {clienteCreate.Error?.Message}");
    }

    var clienteId = clienteCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (clienteId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 7: clienteId nao retornado.");
    }

    var equipamentoRequest = new JsonObject
    {
        ["clienteId"] = clienteId,
        ["tipo"] = "Desktop",
        ["marca"] = "HP",
        ["modelo"] = "EliteDesk",
        ["numeroSerie"] = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
    };

    var equipamentoCreate = await equipamentosApi.CriarAsync(equipamentoRequest, ct);
    if (!equipamentoCreate.Succeeded || equipamentoCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar equipamento para fase 7: {equipamentoCreate.Error?.Message}");
    }

    var equipamentoId = equipamentoCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (equipamentoId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 7: equipamentoId nao retornado.");
    }

    cadastroViewModel.Form.ClienteId = clienteId.ToString();
    cadastroViewModel.Form.EquipamentoId = equipamentoId.ToString();
    cadastroViewModel.Form.Defeito = "Nao inicia sistema";
    cadastroViewModel.Form.Observacoes = "Teste de composicao fase 7";

    var createOs = await cadastroViewModel.SubmitAsync(ct);
    if (!createOs.Succeeded || cadastroViewModel.UltimaCriada is null)
    {
        return Results.Problem($"Falha ao criar OS para fase 7: {createOs.Message}");
    }

    var osId = cadastroViewModel.UltimaCriada.Id;

    var carregar = await detalheViewModel.CarregarAsync(osId, ct);
    if (!carregar.Succeeded)
    {
        return Results.Problem($"Falha ao carregar detalhe da OS na fase 7: {carregar.Message}");
    }

    detalheViewModel.ServicoForm.Descricao = "Diagnostico completo";
    detalheViewModel.ServicoForm.Quantidade = 1;
    detalheViewModel.ServicoForm.ValorUnitario = 120m;
    var servicoResult = await detalheViewModel.AdicionarServicoAsync(ct);
    if (!servicoResult.Succeeded)
    {
        return Results.Problem($"Falha ao adicionar servico na fase 7: {servicoResult.Message}");
    }

    detalheViewModel.ProdutoForm.Descricao = "Fonte ATX";
    detalheViewModel.ProdutoForm.Quantidade = 1;
    detalheViewModel.ProdutoForm.ValorUnitario = 280m;
    var produtoResult = await detalheViewModel.AdicionarProdutoAsync(ct);
    if (!produtoResult.Succeeded)
    {
        return Results.Problem($"Falha ao adicionar produto na fase 7: {produtoResult.Message}");
    }

    detalheViewModel.TaxaForm.Descricao = "Taxa de urgencia";
    detalheViewModel.TaxaForm.Valor = 40m;
    var taxaResult = await detalheViewModel.AdicionarTaxaAsync(ct);
    if (!taxaResult.Succeeded)
    {
        return Results.Problem($"Falha ao adicionar taxa na fase 7: {taxaResult.Message}");
    }

    detalheViewModel.DescontoForm.Tipo = TipoDescontoModel.ValorFixo;
    detalheViewModel.DescontoForm.Valor = 50m;
    var descontoResult = await detalheViewModel.AplicarDescontoAsync(ct);
    if (!descontoResult.Succeeded)
    {
        return Results.Problem($"Falha ao aplicar desconto na fase 7: {descontoResult.Message}");
    }

    detalheViewModel.AnotacaoForm.Texto = "Cliente autorizou reparo.";
    detalheViewModel.AnotacaoForm.Autor = "Operador Web";
    var anotacaoResult = await detalheViewModel.AdicionarAnotacaoAsync(ct);
    if (!anotacaoResult.Succeeded)
    {
        return Results.Problem($"Falha ao adicionar anotacao na fase 7: {anotacaoResult.Message}");
    }

    var detalhe = detalheViewModel.Detalhe;
    if (detalhe is null)
    {
        return Results.Problem("Falha no smoke da fase 7: detalhe final nao carregado.");
    }

    var gatePassed = detalhe.Servicos.Count > 0
        && detalhe.Produtos.Count > 0
        && detalhe.Taxas.Count > 0
        && detalhe.ValorTotal > 0;

    return Results.Ok(new
    {
        ordemServicoId = osId,
        servicos = detalhe.Servicos.Count,
        produtos = detalhe.Produtos.Count,
        taxas = detalhe.Taxas.Count,
        desconto = detalhe.ValorDesconto,
        total = detalhe.ValorTotal,
        gatePassed
    });
});

app.MapGet("/internal/ordens-servico-phase8-smoke", async (
    IClientesApi clientesApi,
    IEquipamentosApi equipamentosApi,
    OrdemServicoCadastroViewModel cadastroViewModel,
    OrdemServicoDetalheViewModel detalheViewModel,
    CancellationToken ct) =>
{
    var clienteRequest = new JsonObject
    {
        ["nome"] = "Cliente Fase 8",
        ["documento"] = Guid.NewGuid().ToString("N")[..11],
        ["telefone"] = "11944443333",
        ["email"] = "fase8@os.local",
        ["endereco"] = "Rua Fase 8"
    };

    var clienteCreate = await clientesApi.CriarAsync(clienteRequest, ct);
    if (!clienteCreate.Succeeded || clienteCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar cliente para fase 8: {clienteCreate.Error?.Message}");
    }

    var clienteId = clienteCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (clienteId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 8: clienteId nao retornado.");
    }

    var equipamentoRequest = new JsonObject
    {
        ["clienteId"] = clienteId,
        ["tipo"] = "Notebook",
        ["marca"] = "Asus",
        ["modelo"] = "VivoBook",
        ["numeroSerie"] = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
    };

    var equipamentoCreate = await equipamentosApi.CriarAsync(equipamentoRequest, ct);
    if (!equipamentoCreate.Succeeded || equipamentoCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar equipamento para fase 8: {equipamentoCreate.Error?.Message}");
    }

    var equipamentoId = equipamentoCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (equipamentoId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 8: equipamentoId nao retornado.");
    }

    cadastroViewModel.Form.ClienteId = clienteId.ToString();
    cadastroViewModel.Form.EquipamentoId = equipamentoId.ToString();
    cadastroViewModel.Form.Defeito = "Nao liga";
    cadastroViewModel.Form.Observacoes = "Teste da fase 8";

    var createOs = await cadastroViewModel.SubmitAsync(ct);
    if (!createOs.Succeeded || cadastroViewModel.UltimaCriada is null)
    {
        return Results.Problem($"Falha ao criar OS para fase 8: {createOs.Message}");
    }

    var osId = cadastroViewModel.UltimaCriada.Id;
    var carregar = await detalheViewModel.CarregarAsync(osId, ct);
    if (!carregar.Succeeded)
    {
        return Results.Problem($"Falha ao carregar detalhe da OS na fase 8: {carregar.Message}");
    }

    var enviar = await detalheViewModel.EnviarOrcamentoAsync(ct);
    if (!enviar.Succeeded)
    {
        return Results.Problem($"Falha ao enviar OS para Orcamento na fase 8: {enviar.Message}");
    }

    detalheViewModel.ServicoForm.Descricao = "Tentativa apos envio";
    detalheViewModel.ServicoForm.Quantidade = 1;
    detalheViewModel.ServicoForm.ValorUnitario = 10m;
    var lockResult = await detalheViewModel.AdicionarServicoAsync(ct);

    var detalhe = detalheViewModel.Detalhe;
    if (detalhe is null)
    {
        return Results.Problem("Falha no smoke da fase 8: detalhe final nao carregado.");
    }

    var gatePassed = detalhe.Status == StatusOsModel.Orcamento
        && detalheViewModel.IsEdicaoBloqueadaAposEnvio
        && !lockResult.Succeeded;

    return Results.Ok(new
    {
        ordemServicoId = osId,
        statusAtual = detalhe.Status,
        edicaoBloqueada = detalheViewModel.IsEdicaoBloqueadaAposEnvio,
        tentativaEdicaoAposEnvioFalhou = !lockResult.Succeeded,
        gatePassed
    });
});

app.MapGet("/internal/ordens-servico-phase9-smoke", async (
    IClientesApi clientesApi,
    IEquipamentosApi equipamentosApi,
    OrdemServicoCadastroViewModel cadastroViewModel,
    OrdemServicoListaViewModel listaViewModel,
    CancellationToken ct) =>
{
    var clienteRequest = new JsonObject
    {
        ["nome"] = "Cliente Fase 9",
        ["documento"] = Guid.NewGuid().ToString("N")[..11],
        ["telefone"] = "11933332222",
        ["email"] = "fase9@os.local",
        ["endereco"] = "Rua Fase 9"
    };

    var clienteCreate = await clientesApi.CriarAsync(clienteRequest, ct);
    if (!clienteCreate.Succeeded || clienteCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar cliente para fase 9: {clienteCreate.Error?.Message}");
    }

    var clienteId = clienteCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (clienteId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 9: clienteId nao retornado.");
    }

    var equipamentoRequest = new JsonObject
    {
        ["clienteId"] = clienteId,
        ["tipo"] = "Desktop",
        ["marca"] = "Lenovo",
        ["modelo"] = "M90",
        ["numeroSerie"] = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
    };

    var equipamentoCreate = await equipamentosApi.CriarAsync(equipamentoRequest, ct);
    if (!equipamentoCreate.Succeeded || equipamentoCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar equipamento para fase 9: {equipamentoCreate.Error?.Message}");
    }

    var equipamentoId = equipamentoCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (equipamentoId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 9: equipamentoId nao retornado.");
    }

    cadastroViewModel.Form.ClienteId = clienteId.ToString();
    cadastroViewModel.Form.EquipamentoId = equipamentoId.ToString();
    cadastroViewModel.Form.Defeito = "Tela azul";
    cadastroViewModel.Form.Observacoes = "Teste de listagem fase 9";

    var createOs = await cadastroViewModel.SubmitAsync(ct);
    if (!createOs.Succeeded || cadastroViewModel.UltimaCriada is null)
    {
        return Results.Problem($"Falha ao criar OS para fase 9: {createOs.Message}");
    }

    var osCriada = cadastroViewModel.UltimaCriada;

    var carregarLista = await listaViewModel.CarregarAsync(1, 1, ct);
    if (!carregarLista.Succeeded)
    {
        return Results.Problem($"Falha ao carregar lista na fase 9: {carregarLista.Message}");
    }

    listaViewModel.NumeroFiltro = osCriada.Numero;
    var filtroNumeroOk = listaViewModel.ItensFiltrados.Any(i => i.Id == osCriada.Id);

    listaViewModel.NumeroFiltro = null;
    listaViewModel.StatusFiltro = osCriada.Status;
    var filtroStatusOk = listaViewModel.ItensFiltrados.Any(i => i.Id == osCriada.Id);

    listaViewModel.StatusFiltro = null;
    listaViewModel.ClienteIdFiltro = clienteId.ToString();
    var filtroClienteOk = listaViewModel.ItensFiltrados.Any(i => i.Id == osCriada.Id);

    var detalheShortcut = $"/ordens-servico/detalhe/{osCriada.Id}";

    var gatePassed = listaViewModel.Itens.Count > 0
        && filtroNumeroOk
        && filtroStatusOk
        && filtroClienteOk;

    return Results.Ok(new
    {
        page = listaViewModel.Page,
        pageSize = listaViewModel.PageSize,
        totalCount = listaViewModel.TotalCount,
        filtroNumeroOk,
        filtroStatusOk,
        filtroClienteOk,
        detalheShortcut,
        gatePassed
    });
});

app.MapGet("/internal/ordens-servico-phase10-smoke", async (
    IClientesApi clientesApi,
    IEquipamentosApi equipamentosApi,
    OrdemServicoCadastroViewModel cadastroViewModel,
    OrdemServicoDetalheViewModel detalheViewModel,
    CancellationToken ct) =>
{
    var clienteRequest = new JsonObject
    {
        ["nome"] = "Cliente Fase 10",
        ["documento"] = Guid.NewGuid().ToString("N")[..11],
        ["telefone"] = "11922221111",
        ["email"] = "fase10@os.local",
        ["endereco"] = "Rua Fase 10"
    };

    var clienteCreate = await clientesApi.CriarAsync(clienteRequest, ct);
    if (!clienteCreate.Succeeded || clienteCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar cliente para fase 10: {clienteCreate.Error?.Message}");
    }

    var clienteId = clienteCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (clienteId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 10: clienteId nao retornado.");
    }

    var equipamentoRequest = new JsonObject
    {
        ["clienteId"] = clienteId,
        ["tipo"] = "Desktop",
        ["marca"] = "Dell",
        ["modelo"] = "OptiPlex",
        ["numeroSerie"] = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
    };

    var equipamentoCreate = await equipamentosApi.CriarAsync(equipamentoRequest, ct);
    if (!equipamentoCreate.Succeeded || equipamentoCreate.Data is null)
    {
        return Results.Problem($"Falha ao criar equipamento para fase 10: {equipamentoCreate.Error?.Message}");
    }

    var equipamentoId = equipamentoCreate.Data["id"]?.GetValue<Guid>() ?? Guid.Empty;
    if (equipamentoId == Guid.Empty)
    {
        return Results.Problem("Falha no smoke test da fase 10: equipamentoId nao retornado.");
    }

    cadastroViewModel.Form.ClienteId = clienteId.ToString();
    cadastroViewModel.Form.EquipamentoId = equipamentoId.ToString();
    cadastroViewModel.Form.Defeito = "Nao liga";
    cadastroViewModel.Form.Observacoes = "Teste da fase 10";

    var createOs = await cadastroViewModel.SubmitAsync(ct);
    if (!createOs.Succeeded || cadastroViewModel.UltimaCriada is null)
    {
        return Results.Problem($"Falha ao criar OS para fase 10: {createOs.Message}");
    }

    var osId = cadastroViewModel.UltimaCriada.Id;
    var carregar = await detalheViewModel.CarregarAsync(osId, ct);
    if (!carregar.Succeeded)
    {
        return Results.Problem($"Falha ao carregar detalhe na fase 10: {carregar.Message}");
    }

    detalheViewModel.ServicoForm.Descricao = "Diagnostico";
    detalheViewModel.ServicoForm.Quantidade = 1;
    detalheViewModel.ServicoForm.ValorUnitario = 120m;
    var addServico = await detalheViewModel.AdicionarServicoAsync(ct);
    if (!addServico.Succeeded)
    {
        return Results.Problem($"Falha ao adicionar servico na fase 10: {addServico.Message}");
    }

    var enviar = await detalheViewModel.EnviarOrcamentoAsync(ct);
    if (!enviar.Succeeded)
    {
        return Results.Problem($"Falha ao enviar para Orcamento na fase 10: {enviar.Message}");
    }

    var aprovar = await detalheViewModel.AprovarAsync(ct);
    if (!aprovar.Succeeded)
    {
        return Results.Problem($"Falha ao aprovar na fase 10: {aprovar.Message}");
    }

    var iniciar = await detalheViewModel.IniciarAndamentoAsync(ct);
    if (!iniciar.Succeeded)
    {
        return Results.Problem($"Falha ao iniciar andamento na fase 10: {iniciar.Message}");
    }

    var concluir = await detalheViewModel.ConcluirAsync(ct);
    if (!concluir.Succeeded)
    {
        return Results.Problem($"Falha ao concluir na fase 10: {concluir.Message}");
    }

    detalheViewModel.PagamentoForm.Meio = MeioPagamentoModel.Pix;
    detalheViewModel.PagamentoForm.Valor = detalheViewModel.TotalFinal;
    detalheViewModel.PagamentoForm.DataPagamento = DateTime.Now;
    var pagamento = await detalheViewModel.RegistrarPagamentoAsync(ct);
    if (!pagamento.Succeeded)
    {
        return Results.Problem($"Falha ao registrar pagamento na fase 10: {pagamento.Message}");
    }

    var entregar = await detalheViewModel.EntregarAsync(ct);
    if (!entregar.Succeeded)
    {
        return Results.Problem($"Falha ao entregar na fase 10: {entregar.Message}");
    }

    var detalhe = detalheViewModel.Detalhe;
    if (detalhe is null)
    {
        return Results.Problem("Falha no smoke da fase 10: detalhe final nao carregado.");
    }

    var gatePassed = detalhe.Status == StatusOsModel.Entregue
        && detalheViewModel.SaldoRestante <= 0
        && detalhe.Pagamentos.Count > 0;

    return Results.Ok(new
    {
        ordemServicoId = osId,
        statusAtual = detalhe.Status,
        valorTotal = detalheViewModel.TotalFinal,
        valorPago = detalheViewModel.ValorPago,
        saldoRestante = detalheViewModel.SaldoRestante,
        pagamentos = detalhe.Pagamentos.Count,
        gatePassed
    });
});

app.MapGet("/internal/web-phase11-ux-smoke", async (
    IApiErrorParser apiErrorParser,
    ClienteCadastroViewModel clienteViewModel,
    IWebHostEnvironment environment,
    CancellationToken ct) =>
{
    var networkError = await apiErrorParser.ParseAsync(HttpStatusCode.ServiceUnavailable, null, ct);
    var friendlyNetworkMessageOk = networkError.Message.Contains("indisponivel", StringComparison.OrdinalIgnoreCase)
        || networkError.Message.Contains("conectar", StringComparison.OrdinalIgnoreCase);

    clienteViewModel.AbrirNovoCliente();
    clienteViewModel.Form.Nome = string.Empty;
    var submitResult = await clienteViewModel.SubmitAsync(ct);
    var submitStateResetOk = !submitResult.Succeeded
        && !clienteViewModel.IsSubmitting
        && clienteViewModel.ValidationErrors.Count > 0;

    var cssPath = Path.Combine(environment.WebRootPath, "app.css");
    var cssContent = await File.ReadAllTextAsync(cssPath, ct);
    var focusVisibleOk = cssContent.Contains(":focus-visible", StringComparison.Ordinal);
    var responsiveOk = cssContent.Contains("@media (max-width: 768px)", StringComparison.Ordinal);

    var gatePassed = friendlyNetworkMessageOk && submitStateResetOk && focusVisibleOk && responsiveOk;

    return Results.Ok(new
    {
        friendlyNetworkMessageOk,
        submitStateResetOk,
        focusVisibleOk,
        responsiveOk,
        gatePassed
    });
});

app.Run();
