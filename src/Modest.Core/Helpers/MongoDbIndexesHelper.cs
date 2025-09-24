using MongoDB.Driver;

namespace Modest.Core.Helpers;

public static class MongoDbIndexesHelper
{
    public static void EnsureCreatingIndexes(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);

        // Find all types implementing IMongoIndexConfigurator
        var configuratorType = typeof(Modest.Core.Data.IMongoIndexConfigurator);
        var types = AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => configuratorType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        foreach (var type in types)
        {
            try
            {
                var configurator = (Modest.Core.Data.IMongoIndexConfigurator)
                    Activator.CreateInstance(type)!;
                configurator.CreateIndexes(database);
            }
            catch (Exception ex)
            {
                // Optionally log or handle errors
                Console.WriteLine($"Failed to create indexes for {type.Name}: {ex.Message}");
            }
        }
    }
}
