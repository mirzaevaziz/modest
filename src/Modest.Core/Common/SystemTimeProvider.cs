namespace Modest.Core.Common;

/// <summary>
/// Default implementation of ITimeProvider that returns the system time.
/// </summary>
public class SystemTimeProvider : ITimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time from the system clock.
    /// </summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
