namespace Application.DTOs.Comuns;

public record PagedResponse<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public int CurrentPage { get; init; }
    
    public PagedResponse(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        CurrentPage = page;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
