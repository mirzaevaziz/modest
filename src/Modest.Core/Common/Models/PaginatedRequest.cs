using FluentValidation;

namespace Modest.Core.Common.Models;

public class PaginatedRequest<TFilter>
{
    private int _pageNumber = 1;
    private int _pageSize = 20;

    public TFilter? Filter { get; set; }

    public int PageNumber
    {
        get => _pageNumber;
        set
        {
            if (value <= 0)
            {
                throw new ValidationException("Page number must be greater than 0.");
            }

            _pageNumber = value;
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value <= 0)
            {
                throw new ValidationException("Page size must be greater than 0.");
            }

            _pageSize = value;
        }
    }
}
