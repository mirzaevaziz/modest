using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modest.Core.Data;
using Modest.Core.Features.References.Product;

namespace Modest.Core;

public static class DependencyInjection
{
    public static void AddCoreServices(this IHostApplicationBuilder builder)
    {
        var settings =
            builder.Configuration.GetSection("MongoDBSettings").Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("MongoDbSetting is not provided.");

        if (!builder.Environment.IsEnvironment("IntegrationTest"))
        {
            if (
                string.IsNullOrWhiteSpace(settings.ConnectionString)
                || string.IsNullOrWhiteSpace(settings.DatabaseName)
            )
            {
                throw new InvalidOperationException("MongoDbSetting values are not provided.");
            }

            builder.Services.AddDbContext<ModestDbContext>(options =>
            {
                options.UseMongoDB(settings.ConnectionString, settings.DatabaseName);
            });
        }

        builder.Services.AddScoped<IProductService, ProductService>();

        // Register validators from the current assembly
        builder.Services.AddValidatorsFromAssemblyContaining<ModestDbContext>();
    }
}
