namespace Modest.Core.Common.Exceptions;

public class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message)
        : base(message) { }
}
