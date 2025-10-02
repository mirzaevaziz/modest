using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modest.Core.Features.References.Product;
using Modest.Data.Features.References.Product;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Modest.Data;

public static class DependencyInjection
{
    public static void AddDataServices(this IHostApplicationBuilder builder)
    {
        var settings =
            builder.Configuration.GetSection("DbConnectionSettings").Get<DbConnectionSettings>()
            ?? throw new InvalidOperationException("DbConnectionSettings is not provided.");

        if (!builder.Environment.IsEnvironment("IntegrationTest"))
        {
            if (
                string.IsNullOrWhiteSpace(settings.ConnectionString)
                || string.IsNullOrWhiteSpace(settings.DatabaseName)
            )
            {
                throw new InvalidOperationException("MongoDbSetting values are not provided.");
            }

            // Register IMongoDatabase
            builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(
                settings.ConnectionString
            ));
            builder.Services.AddScoped(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName)
            );
        }

        builder.Services.AddScoped<IProductRepository, ProductRepository>();

        // Register validators from the current assembly
        builder.Services.AddValidatorsFromAssemblyContaining<ProductRepository>();

        BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.BsonType.String));
    }
}
