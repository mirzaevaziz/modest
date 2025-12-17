namespace Modest.Core.Common;

public static class Constants
{
    public static readonly char[] WordSeparators = { ' ', '\t', '\n', '\r' };

    // Sequence keys for code generation
    public const string ProductSequenceKey = "products";
    public const string SupplierSequenceKey = "suppliers";

    // Code prefixes
    public const string ProductCodePrefix = "SKU-";
    public const string SupplierCodePrefix = "SUP-";
}
