namespace Modest.Core.Common;

public class ItemNotFoundException : Exception
{
    public ItemNotFoundException(string message)
        : base(message) { }
}
