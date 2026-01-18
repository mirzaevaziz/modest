namespace Modest.Core.Common;

/// <summary>
/// Provides the current date and time for the application.
/// This interface allows for better testability by enabling time mocking in tests.
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}