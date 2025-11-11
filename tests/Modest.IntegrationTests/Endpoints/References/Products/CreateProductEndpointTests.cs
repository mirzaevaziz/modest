using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class CreateProductEndpointTests(WebFixture mongoDbFixture)
    : IntegrationTestBase(mongoDbFixture)
{
    [Theory]
    [InlineData(null, null, null, 1)]
    [InlineData(null, "TestMan", "TestLand", 1)]
    [InlineData("", "TestMan", "TestLand", 1)]
    [InlineData("1", "TestMan", "TestLand", 1)]
    [InlineData("12", "TestMan", "TestLand", 1)]
    [InlineData("TestProd", null, "TestLand", 1)]
    [InlineData("TestProd", "", "TestLand", 1)]
    [InlineData("TestProd", "1", "TestLand", 1)]
    [InlineData("TestProd", "12", "TestLand", 1)]
    [InlineData("TestProd", "TestMan", null, 1)]
    [InlineData("TestProd", "TestMan", "", 1)]
    [InlineData("TestProd", "TestMan", "1", 1)]
    [InlineData("TestProd", "TestMan", "12", 1)]
    public async Task Given_InvalidFields_When_CreatingProduct_Then_ReturnsBadRequestAsync(
        string? name,
        string? manufacturer,
        string? country,
        int pieceCountInUnit
    )
    {
        var dto = new ProductCreateDto(name!, manufacturer!, country!, pieceCountInUnit);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_LongName_When_CreatingProduct_Then_ReturnsBadRequestAsync()
    {
        var longStr = new string('a', 1001);
        var dto = new ProductCreateDto(longStr, longStr, longStr, 1);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_LongManufacturer_When_CreatingProduct_Then_ReturnsBadRequestAsync()
    {
        var longStr = new string('a', 1001);
        var dto = new ProductCreateDto("TestProduct", longStr, "TestCountry", 1);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_LongCountry_When_CreatingProduct_Then_ReturnsBadRequestAsync()
    {
        var longStr = new string('a', 1001);
        var dto = new ProductCreateDto("TestProduct", "TestManufacturer", longStr, 1);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_NullDto_When_CreatingProduct_Then_ReturnsBadRequestAsync()
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(new { }).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_ValidData_When_CreatingProduct_Then_ReturnsOkAndProductAsync()
    {
        // Arrange
        var dto = new ProductCreateDto("Test Product", "TestMan", "TestLand", 100);

        // Act & Assert using Alba
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Assert
        var product = await resp.ReadAsJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product!.Name.Should().Be(dto.Name);
        product.Manufacturer.Should().Be(dto.Manufacturer);
        product.Country.Should().Be(dto.Country);
        product.PieceCountInUnit.Should().Be(dto.PieceCountInUnit);

        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        var productInDb = await productRepository.GetProductByIdAsync(product.Id);
        productInDb.Should().NotBeNull();
        productInDb!.Name.Should().Be(dto.Name);
        productInDb.Manufacturer.Should().Be(dto.Manufacturer);
        productInDb.Country.Should().Be(dto.Country);
        productInDb.PieceCountInUnit.Should().Be(dto.PieceCountInUnit);
    }

    [Fact]
    public async Task Given_DuplicateProduct_When_Creating_Then_ReturnsBadRequestAsync()
    {
        var dto = new ProductCreateDto("UniqueProduct", "Man", "Land", 50);
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
    public async Task Given_DeletedDuplicate_When_Creating_Then_ReturnsOkAndProductAsync()
    {
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();

        // Arrange: Add a deleted entity using service
        var dto = new ProductCreateDto("Test Product", "TestMan", "TestLand", 75);
        var entity = await productService.CreateProductAsync(dto);
        await productRepository.DeleteProductAsync(entity.Id);

        // Act: Create a duplicate via the API
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Assert
        var product = await resp.ReadAsJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product.Id.Should().Be(entity.Id);
        product!.Name.Should().Be(dto.Name);
        product.Manufacturer.Should().Be(dto.Manufacturer);
        product.Country.Should().Be(dto.Country);
        product.PieceCountInUnit.Should().Be(dto.PieceCountInUnit);

        var productInDb = await productRepository.GetProductByIdAsync(product.Id);
        productInDb.Should().NotBeNull();
        productInDb!.IsDeleted.Should().BeFalse();
        productInDb.Name.Should().Be(dto.Name);
        productInDb.Manufacturer.Should().Be(dto.Manufacturer);
        productInDb.Country.Should().Be(dto.Country);
        productInDb.PieceCountInUnit.Should().Be(dto.PieceCountInUnit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(10001)]
    [InlineData(10002)]
    [InlineData(20000)]
    public async Task Given_InvalidPieceCount_When_CreatingProduct_Then_ReturnsBadRequestAsync(
        int pieceCountInUnit
    )
    {
        var dto = new ProductCreateDto("TestProduct", "TestMan", "TestLand", pieceCountInUnit);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(5000)]
    [InlineData(10000)]
    public async Task Given_ValidPieceCount_When_CreatingProduct_Then_ReturnsOkAsync(
        int pieceCountInUnit
    )
    {
        var dto = new ProductCreateDto(
            $"Product{pieceCountInUnit}",
            "TestMan",
            "TestLand",
            pieceCountInUnit
        );
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/products");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var product = await resp.ReadAsJsonAsync<ProductDto>();
        product.Should().NotBeNull();
        product!.PieceCountInUnit.Should().Be(pieceCountInUnit);
    }
}
