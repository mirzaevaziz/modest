using Modest.Core.Features.Auth;

namespace Modest.Data.Common;

public class DefaultCurrentUserProvider : ICurrentUserProvider
{
    public UserDto? GetCurrentUser() =>
        new(Guid.CreateVersion7(), "system", "System", "system@email.com");
}
