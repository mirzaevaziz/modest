using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class UpdateProductEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Theory]
    [InlineData(null, null, null)]
    [InlineData(null, "TestMan", "TestLand")]
    [InlineData("", "TestMan", "TestLand")]
    [InlineData("1", "TestMan", "TestLand")]
    [InlineData("12", "TestMan", "TestLand")]
    [InlineData("TestProd", null, "TestLand")]
    [InlineData("TestProd", "", "TestLand")]
    [InlineData("TestProd", "1", "TestLand")]
    [InlineData("TestProd", "12", "TestLand")]
    [InlineData("TestProd", "TestMan", null)]
    [InlineData("TestProd", "TestMan", "")]
    [InlineData("TestProd", "TestMan", "1")]
    [InlineData("TestProd", "TestMan", "12")]
    public async Task UpdateProductEdgeCasesAsync(
        string? name,
        string? manufacturer,
        string? country
    )
    {
        // Arrange: create a valid product using the repository
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        var entity = await productRepository.CreateProductAsync(
            new ProductCreateDto("EdgeName", "EdgeMan", "EdgeLand")
        );
        // Act: try to update with edge case values
        var updateDto = new ProductUpdateDto(entity.Id, name!, manufacturer!, country!);
        var updateResp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task UpdateProductReturnsOkAndUpdatesProductAsync()
    {
        // Arrange: create a valid product using the repository
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        var entity = await productRepository.CreateProductAsync(
            new ProductCreateDto("EdgeName", "EdgeMan", "EdgeLand")
        );
        // Act: update the product
        var updateDto = new ProductUpdateDto(entity.Id, "UpdatedName", "UpdatedMan", "UpdatedLand");
        var updateResp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var updated = await updateResp.ReadAsJsonAsync<ProductDto>();
        updated.Should().NotBeNull();
        updated.Id.Should().Be(entity.Id);
        updated.Name.Should().Be("UpdatedName");
        updated.Manufacturer.Should().Be("UpdatedMan");
        updated.Country.Should().Be("UpdatedLand");
        // Assert in DB
        var inDb = await productRepository.GetProductByIdAsync(entity.Id);
        inDb.Should().NotBeNull();
        inDb!.Name.Should().Be("UpdatedName");
        inDb.Manufacturer.Should().Be("UpdatedMan");
        inDb.Country.Should().Be("UpdatedLand");
    }

    [Fact]
    public async Task UpdateProductNotFoundReturnsNotFoundAsync()
    {
        var updateDto = new ProductUpdateDto(Guid.NewGuid(), "Name", "Man", "Land");
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task UpdateProductDuplicateReturnsBadRequestAsync()
    {
        // Arrange: create two products using the repository
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        var entity1 = await productRepository.CreateProductAsync(
            new ProductCreateDto("Name1", "Man1", "Land1")
        );
        var entity2 = await productRepository.CreateProductAsync(
            new ProductCreateDto("Name2", "Man2", "Land2")
        );
        // Try to update entity2 to have the same fields as entity1 (should fail)
        var updateDto = new ProductUpdateDto(
            entity2.Id,
            entity1.Name,
            entity1.Manufacturer,
            entity1.Country
        );
        var updateResp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task UpdateProductDeletedDuplicateReturnsOkRequestAsync()
    {
        // Arrange: create two products using the repository
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        var entity1 = await productRepository.CreateProductAsync(
            new ProductCreateDto("Name1", "Man1", "Land1")
        );
        var entity2 = await productRepository.CreateProductAsync(
            new ProductCreateDto("Name2", "Man2", "Land2")
        );
        await productRepository.DeleteProductAsync(entity1.Id);
        // Try to update entity2 to have the same fields as entity1
        var updateDto = new ProductUpdateDto(
            entity2.Id,
            entity1.Name,
            entity1.Manufacturer,
            entity1.Country
        );
        var updateResp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var inDb = await productRepository.GetProductByIdAsync(entity1.Id);
        inDb.Should().NotBeNull();
        inDb!.Name.Should().StartWith(entity1.Name + " - Changed ");
        inDb.Manufacturer.Should().Be(entity1.Manufacturer);
        inDb.Country.Should().Be(entity1.Country);

        inDb = await productRepository.GetProductByIdAsync(entity2.Id);
        inDb.Should().NotBeNull();
        inDb!.Name.Should().Be(entity1.Name);
        inDb.Manufacturer.Should().Be(entity1.Manufacturer);
        inDb.Country.Should().Be(entity1.Country);
    }
    // ...existing code...
}
