// using DotNet.Testcontainers.Builders;
// using DotNet.Testcontainers.Containers;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Http.Json;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Modest.Core.Data;
// using MongoDB.Driver;
// using Xunit;

// namespace Modest.IntegrationTests;

// public class WebFixture : IAsyncLifetime
// {
//     // Drop the test database before each test
//     public async Task ResetDatabaseAsync()
//     {
//         var client = new MongoClient(ConnectionString);
//         await client.DropDatabaseAsync(DatabaseName);
//     }

//     private const string DatabaseName = "ModestTestDb";

//     public IContainer MongoDbContainer { get; private set; } = default!;
//     public string ConnectionString { get; private set; } = default!;
//     public Alba.IAlbaHost AlbaHost { get; private set; } = default!;

//     public async Task InitializeAsync()
//     {
//         var containerName = "modest-tests-mongo-shared";
//         MongoDbContainer = new ContainerBuilder()
//             .WithName(containerName)
//             .WithImage("mongo:latest")
//             .WithPortBinding(27017, true)
//             .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "admin1")
//             .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "password1")
//             .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(27017))
//             .Build();

//         await MongoDbContainer.StartAsync();
//         ConnectionString =
//             "mongodb://admin1:password1@localhost:" + MongoDbContainer.GetMappedPublicPort(27017);

//         AlbaHost = await Alba.AlbaHost.For<global::Program>(builder =>
//         {
//             builder.UseEnvironment("IntegrationTest");
//             builder.ConfigureServices(services =>
//             {
//                 services.Configure<JsonOptions>(options =>
//                 {
//                     options.SerializerOptions.WriteIndented = true;
//                     options.SerializerOptions.PropertyNamingPolicy = System
//                         .Text
//                         .Json
//                         .JsonNamingPolicy
//                         .CamelCase;
//                 });

//                 // services.Remove(services.First(s => s.ServiceType == typeof(ModestDbContext)));
//                 services.AddDbContext<ModestDbContext>(options =>
//                 {
//                     options.UseMongoDB(ConnectionString, DatabaseName);
//                 });
//             });
//         });
//         await AlbaHost.StartAsync();
//     }

//     public async Task DisposeAsync()
//     {
//         if (MongoDbContainer != null)
//         {
//             await MongoDbContainer.StopAsync();
//             await MongoDbContainer.DisposeAsync();
//         }

//         if (AlbaHost != null)
//         {
//             await AlbaHost.DisposeAsync();
//         }
//     }
// }

// [CollectionDefinition("MongoDb collection")]
// public class WebCollectionFixture : ICollectionFixture<WebFixture> { }
