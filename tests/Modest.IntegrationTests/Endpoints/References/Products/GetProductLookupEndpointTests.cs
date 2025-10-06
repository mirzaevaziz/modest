using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
        // Add 15 products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        for (var i = 1; i <= 15; i++)
        {
            await productService.CreateProductAsync(
                new ProductCreateDto($"Prod{i}", $"Man{i}", $"Land{i}")
            );
        }

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
        // Add products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("Alpha", "AAA", "XXXX"));
        await productService.CreateProductAsync(new ProductCreateDto("Beta", "BAA", "YYYY"));
        await productService.CreateProductAsync(new ProductCreateDto("Gamma", "CAA", "ZZZZ"));
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
