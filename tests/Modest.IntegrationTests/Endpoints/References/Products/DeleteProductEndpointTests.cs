using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class DeleteProductEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task Given_ValidProduct_When_Deleting_Then_ReturnsOkAndSoftDeletesAsync()
    {
        // Arrange: create a product using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();

        var entity = await productService.CreateProductAsync(
            new ProductCreateDto("DeleteMe", "DeleteMan", "DeleteLand", 1)
        );
        // Act: delete the product
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/products/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        // Assert: product is deleted
        var inDb = await productService.GetProductByIdAsync(entity.Id);
        inDb.Should().NotBeNull();
        inDb!.IsDeleted.Should().BeTrue();
        inDb.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_NonExistentId_When_Deleting_Then_ReturnsOkFalseAsync()
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
    public async Task Given_AlreadyDeletedProduct_When_DeletingAgain_Then_ReturnsOkFalseAsync()
    {
        // Arrange: create a product
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();

        var entity = await productService.CreateProductAsync(
            new ProductCreateDto("DeleteMe", "DeleteMan", "DeleteLand", 1)
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
