﻿using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modest.Core.Features.Auth;
using Modest.Core.Features.References.Product;
using Modest.Core.Features.Utils.SequenceNumber;
using Modest.Data.Common;
using Modest.Data.Features.References.Product;
using Modest.Data.Features.Utils.SequenceNumber;
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

        // Register current user provider
        builder.Services.AddScoped<ICurrentUserProvider, DefaultCurrentUserProvider>();

        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ISequenceNumberRepository, SequenceNumberRepository>();

        // Register validators from the current assembly
        builder.Services.AddValidatorsFromAssemblyContaining<ProductRepository>();

        BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.BsonType.String));
    }
}
