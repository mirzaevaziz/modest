// using System.Net;
// using FluentAssertions;
// using Modest.Core.Features.References.Product;
// using Xunit;

// namespace Modest.IntegrationTests.Endpoints.References.Products;

// public class CreateProductEndpointTests(WebFixture mongoDbFixture)
//     : IntegrationTestBase(mongoDbFixture)
// {
//     [Theory]
//     [InlineData(null, null, null)]
//     [InlineData(null, "TestMan", "TestLand")]
//     [InlineData("", "TestMan", "TestLand")]
//     [InlineData("1", "TestMan", "TestLand")]
//     [InlineData("12", "TestMan", "TestLand")]
//     [InlineData("TestProd", null, "TestLand")]
//     [InlineData("TestProd", "", "TestLand")]
//     [InlineData("TestProd", "1", "TestLand")]
//     [InlineData("TestProd", "12", "TestLand")]
//     [InlineData("TestProd", "TestMan", null)]
//     [InlineData("TestProd", "TestMan", "")]
//     [InlineData("TestProd", "TestMan", "1")]
//     [InlineData("TestProd", "TestMan", "12")]
//     public async Task CreateProductInvalidFieldsReturnsBadRequestAsync(
//         string? name,
//         string? manufacturer,
//         string? country
//     )
//     {
//         var dto = new ProductCreateDto(name!, manufacturer!, country!);
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(dto).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
//         });
//     }

//     [Fact]
//     public async Task CreateProductLongStringsReturnsBadRequestAsync()
//     {
//         var longStr = new string('a', 1001);
//         var dto = new ProductCreateDto(longStr, longStr, longStr);
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(dto).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
//         });
//     }

//     [Fact]
//     public async Task CreateProductLongManufacturerReturnsBadRequestAsync()
//     {
//         var longStr = new string('a', 1001);
//         var dto = new ProductCreateDto("TestProduct", longStr, "TestCountry");
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(dto).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
//         });
//     }

//     [Fact]
//     public async Task CreateProductLongCountryReturnsBadRequestAsync()
//     {
//         var longStr = new string('a', 1001);
//         var dto = new ProductCreateDto("TestProduct", "TestManufacturer", longStr);
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(dto).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
//         });
//     }

//     [Fact]
//     public async Task CreateProductNullDtoReturnsBadRequestAsync()
//     {
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(new { }).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
//         });
//     }

//     [Fact]
//     public async Task CreateProductReturnsOkAndProductAsync()
//     {
//         // Arrange
//         var dto = new ProductCreateDto("Test Product", "TestMan", "TestLand");

//         // Act & Assert using Alba
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(dto).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });

//         var product = await resp.ReadAsJsonAsync<ProductDto>();
//         product.Should().NotBeNull();
//         product.Id.Should().NotBeEmpty();
//         product!.Name.Should().Be(dto.Name);
//         product.Manufacturer.Should().Be(dto.Manufacturer);
//         product.Country.Should().Be(dto.Country);

//         ModestDbContext.ChangeTracker.Clear();
//         var productInDb = await ModestDbContext.Products.FindAsync(product.Id);
//         productInDb.Should().NotBeNull();
//         productInDb!.Name.Should().Be(dto.Name);
//         productInDb.Manufacturer.Should().Be(dto.Manufacturer);
//         productInDb.Country.Should().Be(dto.Country);
//     }

//     [Fact]
//     public async Task CreateProductDuplicateReturnsBadRequestAsync()
//     {
//         var dto = new ProductCreateDto("UniqueProduct", "Man", "Land");
//         // First creation should succeed
//         var resp1 = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(dto).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         // Second creation should fail (assuming unique constraint)
//         var resp2 = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(dto).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
//         });
//     }

//     [Fact]
//     public async Task CreateProductDeletedDuplicateReturnsOkAndProductAsync()
//     {
//         // Arrange: Add a deleted entity directly via DbContext
//         var deletedEntity = new ProductEntity
//         {
//             Id = Guid.NewGuid(),
//             Name = "Test Product",
//             Manufacturer = "TestMan",
//             Country = "TestLand",
//         };
//         ModestDbContext.Products.Add(deletedEntity);
//         await ModestDbContext.SaveChangesAsync();
//         ModestDbContext.Products.Remove(deletedEntity);
//         await ModestDbContext.SaveChangesAsync();

//         // Act: Create a duplicate via the API
//         var dto = new ProductCreateDto("Test Product", "TestMan", "TestLand");
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Post.Json(dto).ToUrl("/api/references/products");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });

//         var product = await resp.ReadAsJsonAsync<ProductDto>();
//         product.Should().NotBeNull();
//         product.Id.Should().Be(deletedEntity.Id);
//         product!.Name.Should().Be(dto.Name);
//         product.Manufacturer.Should().Be(dto.Manufacturer);
//         product.Country.Should().Be(dto.Country);

//         ModestDbContext.ChangeTracker.Clear();
//         var productInDb = await ModestDbContext.Products.FindAsync(product.Id);
//         productInDb.Should().NotBeNull();
//         productInDb!.IsDeleted.Should().BeFalse();
//         productInDb.Name.Should().Be(dto.Name);
//         productInDb.Manufacturer.Should().Be(dto.Manufacturer);
//         productInDb.Country.Should().Be(dto.Country);
//     }
// }
