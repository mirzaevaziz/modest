using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class GetProductByIdEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task GetProductByIdReturnsProductAsync()
    {
        // Arrange: create a product using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        var entity = await productService.CreateProductAsync(
            new ProductCreateDto("TestProd", "TestMan", "TestLand", 1)
        );
        // Act
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<ProductDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be(entity.Name);
        result.Manufacturer.Should().Be(entity.Manufacturer);
        result.Country.Should().Be(entity.Country);
    }

    [Fact]
    public async Task GetProductByIdReturnsNotFoundAsync()
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/{Guid.NewGuid()}");
            api.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }
}
