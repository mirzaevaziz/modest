using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modest.Core.Features.References.Product;
using Modest.Core.Features.References.Supplier;
using Modest.Core.Features.Utils.SequenceNumber;

namespace Modest.Core;

public static class DependencyInjection
{
    public static void AddCoreServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ISupplierService, SupplierService>();
        builder.Services.AddScoped<ISequenceNumberService, SequenceNumberService>();

        // Register validators from the current assembly
        builder.Services.AddValidatorsFromAssemblyContaining<ProductService>();
    }
}
