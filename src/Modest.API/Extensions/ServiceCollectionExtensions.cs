using FluentValidation;
using Modest.Core.Common;
using Modest.Core.Features.Auth;
using Modest.Core.Features.References.Product;
using Modest.Core.Features.References.Supplier;
using Modest.Core.Features.Utils.SequenceNumber;
using Modest.Data;
using Modest.Data.Common;
using Modest.Data.Features.References.Product;
using Modest.Data.Features.References.Supplier;
using Modest.Data.Features.Utils.SequenceNumber;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Modest.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // Register time provider
        services.AddSingleton<ITimeProvider, SystemTimeProvider>();

        // Register services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ISequenceNumberService, SequenceNumberService>();

        // Register validators from Modest.Core assembly
        services.AddValidatorsFromAssemblyContaining<ProductService>();

        return services;
    }

    public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        var settings =
            configuration.GetSection("DbConnectionSettings").Get<DbConnectionSettings>()
            ?? throw new InvalidOperationException("DbConnectionSettings is not provided.");

        if (!environment.IsEnvironment("IntegrationTest"))
        {
            if (
                string.IsNullOrWhiteSpace(settings.ConnectionString)
                || string.IsNullOrWhiteSpace(settings.DatabaseName)
            )
            {
                throw new InvalidOperationException("MongoDbSetting values are not provided.");
            }

            // Register IMongoDatabase
            services.AddSingleton<IMongoClient>(sp => new MongoClient(settings.ConnectionString));
            services.AddScoped(sp =>
                sp.GetRequiredService<IMongoClient>().GetDatabase(settings.DatabaseName)
            );
        }

        // Register current user provider
        services.AddScoped<ICurrentUserProvider, DefaultCurrentUserProvider>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ISequenceNumberRepository, SequenceNumberRepository>();

        // Configure MongoDB conventions
        var conventionPack = new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String), // Store all enums as strings
            new IgnoreExtraElementsConvention(true), // Apply to all types
        };
        ConventionRegistry.Register("EnumStringConvention", conventionPack, type => true);
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

        return services;
    }
}
