using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class GetCountryLookupEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task Given_NoProducts_When_GettingCountryLookup_Then_ReturnsEmptyAsync()
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
            api.Get.Url($"/api/references/products/countries?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<string>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_FifteenProducts_When_GettingCountryLookup_Then_ReturnsPagedDistinctResultsAsync()
    {
        // Add 15 products, 5 unique countries using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        var countries = new[] { "XXXX", "YYYY", "ZZZZ", "WWWW", "QQQQ" };
        for (var i = 1; i <= 15; i++)
        {
            await productService.CreateProductAsync(
                new ProductCreateDto($"Prod{i}", $"Man{i}", countries[(i - 1) % 5], 1)
            );
        }
        // Page 1, size 3
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/countries?pageNumber=1&pageSize=3");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<string>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(3);
        result.TotalCount.Should().Be(5);
        result.Items.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task Given_SearchFilter_When_GettingCountryLookup_Then_ReturnsFilteredAsync()
    {
        // Add products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(
            new ProductCreateDto("Alpha", "AAAA", "AlphaLand", 1)
        );
        await productService.CreateProductAsync(
            new ProductCreateDto("Beta", "BBBB", "BetaLand", 1)
        );
        await productService.CreateProductAsync(
            new ProductCreateDto("Gamma", "CCCC", "GammaLand", 1)
        );
        // Search for 'Beta'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/countries?filter=Beta&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<string>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Should().Be("BetaLand");
        result.TotalCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    [InlineData(1, -5)]
    public async Task Given_InvalidPaging_When_GettingCountryLookup_Then_ReturnsBadRequestAsync(
        int page,
        int pageSize
    )
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products/countries?pageNumber={page}&pageSize={pageSize}"
            );
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }
}
