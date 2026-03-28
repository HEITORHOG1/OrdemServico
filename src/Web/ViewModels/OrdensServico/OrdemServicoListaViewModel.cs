using System.Text.Json;
using Web.Models;
using Web.Models.Responses;
using Web.Services.Api;
using Web.State;
using Web.ViewModels.Foundation;

namespace Web.ViewModels.OrdensServico;

public sealed class OrdemServicoListaViewModel : ViewModelBase
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IOrdensServicoApi _ordensServicoApi;

    private int _page = 1;
    private int _pageSize = 10;
    private int _totalCount;
    private int _totalPages;
    private string? _numeroFiltro;
    private string? _clienteIdFiltro;
    private StatusOsModel? _statusFiltro;

    public OrdemServicoListaViewModel(IOrdensServicoApi ordensServicoApi)
    {
        _ordensServicoApi = ordensServicoApi;
    }

    public List<OrdemServicoResumoResponseModel> Itens { get; } = new();

    public int Page
    {
        get => _page;
        private set => SetProperty(ref _page, value);
    }

    public int PageSize
    {
        get => _pageSize;
        private set => SetProperty(ref _pageSize, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        private set => SetProperty(ref _totalPages, value);
    }

    public string? NumeroFiltro
    {
        get => _numeroFiltro;
        set => SetProperty(ref _numeroFiltro, value);
    }

    public string? ClienteIdFiltro
    {
        get => _clienteIdFiltro;
        set => SetProperty(ref _clienteIdFiltro, value);
    }

    public StatusOsModel? StatusFiltro
    {
        get => _statusFiltro;
        set => SetProperty(ref _statusFiltro, value);
    }

    public IReadOnlyList<StatusOsModel> StatusDisponiveis { get; } = Enum.GetValues<StatusOsModel>();

    public IReadOnlyList<OrdemServicoResumoResponseModel> ItensFiltrados
        => Itens
            .Where(PassaNumero)
            .Where(PassaCliente)
            .Where(PassaStatus)
            .ToList();

    public async Task<OperationResult> CarregarAsync(int? page = null, int? pageSize = null, CancellationToken cancellationToken = default)
    {
        if (page.HasValue && page.Value > 0)
        {
            Page = page.Value;
        }

        if (pageSize.HasValue && pageSize.Value > 0)
        {
            PageSize = pageSize.Value;
        }

        SetLoadingState();

        var response = await _ordensServicoApi.ListarAsync(Page, PageSize, cancellationToken);
        if ((!response.Succeeded || response.Data is null) && PageSize > 1)
        {
            // Some environments may contain legacy rows that break list projection on larger pages.
            // Retry with a smaller page to keep the acompanhamento screen operational.
            var retry = await _ordensServicoApi.ListarAsync(Page, 1, cancellationToken);
            if (retry.Succeeded && retry.Data is not null)
            {
                response = retry;
                PageSize = 1;
            }
        }

        if (!response.Succeeded || response.Data is null)
        {
            var message = response.Error?.Message ?? "Falha ao carregar a lista de OS.";
            SetErrorState(message);
            return OperationResult.Failure(message, response.Error?.ValidationErrors);
        }

        var paged = response.Data.Deserialize<PagedResponseModel<OrdemServicoResumoResponseModel>>(SerializerOptions);
        if (paged is null)
        {
            const string message = "Falha ao processar a resposta de listagem de OS.";
            SetErrorState(message);
            return OperationResult.Failure(message);
        }

        Itens.Clear();
        Itens.AddRange(paged.Items);

        TotalCount = paged.TotalCount;
        Page = paged.Page > 0 ? paged.Page : Page;
        PageSize = paged.PageSize > 0 ? paged.PageSize : PageSize;
        TotalPages = paged.TotalPages > 0
            ? paged.TotalPages
            : (int)Math.Ceiling((double)Math.Max(TotalCount, 1) / Math.Max(PageSize, 1));

        SetSuccessState();
        return OperationResult.Success();
    }

    public async Task<OperationResult> ProximaPaginaAsync(CancellationToken cancellationToken = default)
    {
        if (Page >= TotalPages)
        {
            return OperationResult.Success();
        }

        return await CarregarAsync(Page + 1, null, cancellationToken);
    }

    public async Task<OperationResult> PaginaAnteriorAsync(CancellationToken cancellationToken = default)
    {
        if (Page <= 1)
        {
            return OperationResult.Success();
        }

        return await CarregarAsync(Page - 1, null, cancellationToken);
    }

    public void LimparFiltros()
    {
        NumeroFiltro = null;
        ClienteIdFiltro = null;
        StatusFiltro = null;
    }

    private bool PassaNumero(OrdemServicoResumoResponseModel item)
        => string.IsNullOrWhiteSpace(NumeroFiltro)
           || item.Numero.Contains(NumeroFiltro.Trim(), StringComparison.OrdinalIgnoreCase);

    private bool PassaCliente(OrdemServicoResumoResponseModel item)
        => string.IsNullOrWhiteSpace(ClienteIdFiltro)
           || item.ClienteId.ToString().Contains(ClienteIdFiltro.Trim(), StringComparison.OrdinalIgnoreCase);

    private bool PassaStatus(OrdemServicoResumoResponseModel item)
        => !StatusFiltro.HasValue || item.Status == StatusFiltro.Value;
}
