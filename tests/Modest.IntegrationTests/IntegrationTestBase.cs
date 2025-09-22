// This file sets up Testcontainers and provides base utilities for integration tests.
using System.Text.Json;
using Alba;
// using DotNet.Testcontainers.Builders;
// using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Data;
using Xunit;

namespace Modest.IntegrationTests;

// Use the shared MongoDbFixture for all tests in this base class
[Collection("MongoDb collection")]
public abstract class IntegrationTestBase(WebFixture WebFixture)
{
    private ModestDbContext? _modestDbContext;

    protected IAlbaHost AlbaHost => WebFixture.AlbaHost;

    public ModestDbContext ModestDbContext
    {
        get { return _modestDbContext ??= AlbaHost.Services.GetRequiredService<ModestDbContext>(); }
    }
}
