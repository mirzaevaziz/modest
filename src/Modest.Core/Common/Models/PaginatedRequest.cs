namespace Modest.Core.Common.Models;

public class PaginatedRequest<TFilter>
{
    public TFilter? Filter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
