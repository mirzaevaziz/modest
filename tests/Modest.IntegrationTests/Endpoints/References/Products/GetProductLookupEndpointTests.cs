using System.Net;
using FluentAssertions;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class GetProductLookupEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task GetProductLookupReturnsEmptyAsync()
    {
        // No products in DB
        var req = new PaginatedRequest<string>
        {
            Filter = null,
            PageNumber = 1,
            PageSize = 10,
        };
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<LookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetProductLookupReturnsPagedResultsAsync()
    {
        // Add 15 products
        for (var i = 1; i <= 15; i++)
        {
            ModestDbContext.Products.Add(
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = $"Prod{i}",
                    Manufacturer = $"Man{i}",
                    Country = $"Land{i}",
                }
            );
        }

        await ModestDbContext.SaveChangesAsync();

        // Page 1, size 10
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<LookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(10);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task GetProductLookupWithSearchReturnsFilteredAsync()
    {
        // Add products
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Alpha",
                Manufacturer = "A",
                Country = "X",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Beta",
                Manufacturer = "B",
                Country = "Y",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Gamma",
                Manufacturer = "C",
                Country = "Z",
            }
        );
        await ModestDbContext.SaveChangesAsync();
        // Search for 'lph'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?filter=lph&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<LookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);

        result.Items[0].Name.Should().Contain("Alpha");

        result.TotalCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    [InlineData(1, -5)]
    public async Task GetProductLookupInvalidPagingReturnsEmptyAsync(int page, int pageSize)
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?page={page}&pageSize={pageSize}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<LookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }
}
