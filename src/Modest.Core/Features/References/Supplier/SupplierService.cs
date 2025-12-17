using Microsoft.Extensions.Logging;
using Modest.Core.Common;
using Modest.Core.Common.Models;
using Modest.Core.Features.Utils.SequenceNumber;
using Modest.Core.Helpers;

namespace Modest.Core.Features.References.Supplier;

public interface ISupplierService
{
    Task<PaginatedResponse<SupplierDto>> GetAllSuppliersAsync(
        PaginatedRequest<SupplierFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    );
    Task<PaginatedResponse<SupplierLookupDto>> GetSupplierLookupDtosAsync(
        PaginatedRequest<string> request
    );
    Task<SupplierDto?> GetSupplierByIdAsync(Guid id);
    Task<SupplierDto> CreateSupplierAsync(SupplierCreateDto supplierCreateDto);
    Task<SupplierDto> UpdateSupplierAsync(SupplierUpdateDto supplierUpdateDto);
    Task<bool> DeleteSupplierAsync(Guid id);
}

public class SupplierService(
    ISupplierRepository supplierRepository,
    ISequenceNumberService sequenceNumberService,
    IServiceProvider serviceProvider,
    ILogger<SupplierService> logger
) : ISupplierService
{
    public async Task<PaginatedResponse<SupplierDto>> GetAllSuppliersAsync(
        PaginatedRequest<SupplierFilter> request,
        IEnumerable<SortFieldRequest>? sortFields
    )
    {
        return await supplierRepository.GetAllSuppliersAsync(request, sortFields);
    }

    public async Task<PaginatedResponse<SupplierLookupDto>> GetSupplierLookupDtosAsync(
        PaginatedRequest<string> request
    )
    {
        return await supplierRepository.GetSupplierLookupDtosAsync(request);
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(Guid id)
    {
        return await supplierRepository.GetSupplierByIdAsync(id);
    }

    public async Task<SupplierDto> CreateSupplierAsync(SupplierCreateDto supplierCreateDto)
    {
        SupplierServiceLog.CreatingSupplier(
            logger,
            supplierCreateDto.Name,
            supplierCreateDto.ContactPerson,
            supplierCreateDto.Phone,
            null
        );
        ValidationHelper.ValidateAndThrow(supplierCreateDto, serviceProvider);

        // Generate supplier code using sequence service
        var sequenceNumber = await sequenceNumberService.GetNextAsync(
            Constants.SupplierSequenceKey
        );
        var code = $"{Constants.SupplierCodePrefix}{sequenceNumber:D6}"; // Format: SUP-000001

        var entity = await supplierRepository.CreateSupplierAsync(supplierCreateDto, code);

        SupplierServiceLog.SupplierCreated(logger, entity.Id, entity.Name, null);
        return entity;
    }

    public async Task<SupplierDto> UpdateSupplierAsync(SupplierUpdateDto supplierUpdateDto)
    {
        SupplierServiceLog.UpdatingSupplier(
            logger,
            supplierUpdateDto.Id,
            supplierUpdateDto.Name,
            supplierUpdateDto.ContactPerson,
            supplierUpdateDto.Phone,
            null
        );
        ValidationHelper.ValidateAndThrow(supplierUpdateDto, serviceProvider);

        var entity = await supplierRepository.UpdateSupplierAsync(supplierUpdateDto);

        SupplierServiceLog.SupplierUpdated(logger, entity.Id, entity.Name, null);
        return entity;
    }

    public async Task<bool> DeleteSupplierAsync(Guid id)
    {
        SupplierServiceLog.DeletingSupplier(logger, id, null);

        var result = await supplierRepository.DeleteSupplierAsync(id);
        if (result)
        {
            SupplierServiceLog.SupplierDeleted(logger, id, null);
        }
        else
        {
            SupplierServiceLog.SupplierDeleteFailed(logger, id, null);
        }

        return result;
    }
}
