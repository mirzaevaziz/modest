using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modest.Core.Features.References.Product;

namespace Modest.Core;

public static class DependencyInjection
{
    public static void AddCoreServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped<IProductService, ProductService>();

        // Register validators from the current assembly
        builder.Services.AddValidatorsFromAssemblyContaining<ProductService>();
    }
}
