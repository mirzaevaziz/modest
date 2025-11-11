using Modest.Core.Common.Models;

namespace Modest.Core.Features.References.Supplier;

public interface ISupplierRepository
{
    Task<PaginatedResponse<SupplierDto>> GetAllSuppliersAsync(
        PaginatedRequest<SupplierFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    );
    Task<PaginatedResponse<SupplierLookupDto>> GetSupplierLookupDtosAsync(
        PaginatedRequest<string> request
    );
    Task<SupplierDto?> GetSupplierByIdAsync(Guid id);
    Task<SupplierDto> CreateSupplierAsync(SupplierCreateDto supplierCreateDto, string code);
    Task<SupplierDto> UpdateSupplierAsync(SupplierUpdateDto supplierUpdateDto);
    Task<bool> DeleteSupplierAsync(Guid id);
}
