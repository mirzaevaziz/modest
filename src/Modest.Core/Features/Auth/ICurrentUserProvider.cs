namespace Modest.Core.Features.Auth;

public interface ICurrentUserProvider
{
    UserDto? GetCurrentUser();
}
