namespace Modest.Core.Features.Utils.SequenceNumber;

public interface ISequenceNumberService
{
    /// <summary>
    /// Gets the next sequence number for the given prefix.
    /// </summary>
    /// <param name="prefix">The prefix for which to generate the sequence number.</param>
    /// <returns>The next sequence number for the prefix.</returns>
    Task<long> GetNextAsync(string prefix);
}

public class SequenceNumberService : ISequenceNumberService
{
    private readonly ISequenceNumberRepository _repository;

    public SequenceNumberService(ISequenceNumberRepository repository)
    {
        _repository = repository;
    }

    public async Task<long> GetNextAsync(string prefix)
    {
        return await _repository.GetNextSequenceAsync(prefix);
    }
}
