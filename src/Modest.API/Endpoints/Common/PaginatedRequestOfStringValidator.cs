using FluentValidation;
using Modest.Core.Common.Models;

namespace Modest.API.Endpoints.Common;

public class StringPaginatedRequestValidator : AbstractValidator<PaginatedRequest<string>>
{
    public StringPaginatedRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page size must be greater than 0.");
    }
}
