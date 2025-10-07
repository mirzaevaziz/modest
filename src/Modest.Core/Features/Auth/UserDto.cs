namespace Modest.Core.Features.Auth;

public record UserDto(Guid Id, string Username, string? FullName, string? Email);
