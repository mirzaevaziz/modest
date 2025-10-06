using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class DeleteProductEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task DeleteProductReturnsOkAndDeletesProductAsync()
    {
        // Arrange: create a product using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();

        var entity = await productService.CreateProductAsync(
            new ProductCreateDto("DeleteMe", "DeleteMan", "DeleteLand")
        );
        // Act: delete the product
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/products/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        // Assert: product is deleted
        var inDb = await productRepository.GetProductByIdAsync(entity.Id);
        inDb.Should().NotBeNull();
        inDb!.IsDeleted.Should().BeTrue();
        inDb.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteProductNotFoundReturnsOkFalseAsync()
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/products/{Guid.NewGuid()}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsTextAsync();
        result.Should().Be("false");
    }

    [Fact]
    public async Task DeleteProductTwiceReturnsOkFalseSecondTimeAsync()
    {
        // Arrange: create a product
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();

        var entity = await productService.CreateProductAsync(
            new ProductCreateDto("DeleteMe", "DeleteMan", "DeleteLand")
        );
        // Act: delete once
        var resp1 = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/products/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        // Act: delete again
        var resp2 = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/products/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp2.ReadAsTextAsync();
        result.Should().Be("false");
    }
}
