namespace ECommerce.Api.Common;

public sealed record PaginationRequest(int Page = 1, int PageSize = 20)
{
    public int Offset => (Math.Max(Page, 1) - 1) * Math.Clamp(PageSize, 1, 100);
    public int Limit => Math.Clamp(PageSize, 1, 100);
}

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
