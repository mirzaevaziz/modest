// This file sets up Testcontainers and provides base utilities for integration tests.
using Alba;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Modest.IntegrationTests;

// Use the shared MongoDbFixture for all tests in this base class
[Collection("MongoDb collection")]
public abstract class IntegrationTestBase
{
    protected WebFixture WebFixture { get; }
    protected IAlbaHost AlbaHost => WebFixture.AlbaHost;

    public IntegrationTestBase(WebFixture webFixture)
    {
        WebFixture = webFixture;
        // Drop the database before each test class instance
        webFixture.ResetDatabaseAsync().GetAwaiter().GetResult();
    }
}
