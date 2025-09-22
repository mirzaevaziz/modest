using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests;

public class CreateProductEndpointTests(WebFixture mongoDbFixture)
    : IntegrationTestBase(mongoDbFixture)
{
    [Theory]
    [InlineData(null, "TestMan", "TestLand")]
    [InlineData("", "TestMan", "TestLand")]
    [InlineData("12", "TestMan", "TestLand")]
    public async Task CreateProductInvalidFieldsReturnsBadRequestAsync(
        string? name,
        string? manufacturer,
        string? country
    )
    {
        var dto = new ProductCreateDto(name!, manufacturer, country);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task CreateProductEmptyDtoReturnsBadRequestAsync()
    {
        var dto = new ProductCreateDto(null!, null!, null!);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task CreateProductDuplicateReturnsBadRequestAsync()
    {
        var dto = new ProductCreateDto("UniqueProduct", "Man", "Land");
        // First creation should succeed
        var resp1 = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        // Second creation should fail (assuming unique constraint)
        var resp2 = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task CreateProductLongStringsReturnsBadRequestAsync()
    {
        var longStr = new string('a', 1001);
        var dto = new ProductCreateDto(longStr, longStr, longStr);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task CreateProductNullDtoReturnsBadRequestAsync()
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(new { }).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task CreateProductReturnsOkAndProductAsync()
    {
        // Arrange
        var dto = new ProductCreateDto("Test Product", "TestMan", "TestLand");

        // Act & Assert using Alba
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var product = await resp.ReadAsJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product!.Name.Should().Be(dto.Name);
        product.Manufacturer.Should().Be(dto.Manufacturer);
        product.Country.Should().Be(dto.Country);

        var productInDb = await ModestDbContext.Products.FindAsync(product.Id);
        productInDb.Should().NotBeNull();
        productInDb!.Name.Should().Be(dto.Name);
        productInDb.Manufacturer.Should().Be(dto.Manufacturer);
        productInDb.Country.Should().Be(dto.Country);
    }
}
