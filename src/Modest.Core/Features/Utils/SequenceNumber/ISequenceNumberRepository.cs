namespace Modest.Core.Features.Utils.SequenceNumber;

public interface ISequenceNumberRepository
{
    /// <summary>
    /// Gets the next sequence number for the given prefix.
    /// </summary>
    /// <param name="prefix">The prefix for which to generate the sequence number.</param>
    /// <returns>The next sequence number for the prefix.</returns>
    Task<long> GetNextSequenceAsync(string prefix);
}
