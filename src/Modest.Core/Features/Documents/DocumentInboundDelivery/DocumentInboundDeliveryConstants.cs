namespace Modest.Core.Features.Documents.DocumentInboundDelivery;

public static class DocumentInboundDeliveryConstants
{
    public const int NumberMaxLength = 50;
    public const int NumberMinLength = 1;
    public const int SupplierNameMaxLength = 250;
    public const int SupplierNameMinLength = 2;
    public const int SupplierCodeMaxLength = 50;
    public const int SupplierCodeMinLength = 1;
    public const int SupplierDocumentNumberMaxLength = 50;
    public const int CommentMaxLength = 1000;
    public const decimal MinQuantity = 0.001m;
    public const decimal MaxQuantity = 999999.999m;
    public const decimal MinPrice = 0.01m;
    public const decimal MaxPrice = 999999999.99m;
    public const decimal MinVAT = 0m;
    public const decimal MaxVAT = 1m;
}
