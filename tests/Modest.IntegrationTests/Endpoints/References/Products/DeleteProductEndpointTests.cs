// using System.Net;
// using FluentAssertions;
// using Modest.Core.Features.References.Product;
// using Xunit;

// namespace Modest.IntegrationTests.Endpoints.References.Products;

// public class DeleteProductEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
// {
//     [Fact]
//     public async Task DeleteProductReturnsOkAndDeletesProductAsync()
//     {
//         // Arrange: create a product directly in the database
//         var entity = new ProductEntity
//         {
//             Id = Guid.NewGuid(),
//             Name = "DeleteMe",
//             Manufacturer = "DeleteMan",
//             Country = "DeleteLand",
//         };
//         ModestDbContext.Products.Add(entity);
//         await ModestDbContext.SaveChangesAsync();
//         // Act: delete the product
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Delete.Url($"/api/references/products/{entity.Id}");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         // Assert: product is deleted
//         ModestDbContext.ChangeTracker.Clear();
//         var inDb = await ModestDbContext.Products.FindAsync(entity.Id);
//         inDb.Should().NotBeNull();
//         inDb!.IsDeleted.Should().BeTrue();
//         inDb.DeletedAt.Should().NotBeNull();
//     }

//     [Fact]
//     public async Task DeleteProductNotFoundReturnsOkFalseAsync()
//     {
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Delete.Url($"/api/references/products/{Guid.NewGuid()}");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         var result = await resp.ReadAsTextAsync();
//         result.Should().Be("false");
//     }

//     [Fact]
//     public async Task DeleteProductTwiceReturnsOkFalseSecondTimeAsync()
//     {
//         // Arrange: create a product
//         var entity = new ProductEntity
//         {
//             Id = Guid.NewGuid(),
//             Name = "DeleteTwice",
//             Manufacturer = "DeleteMan",
//             Country = "DeleteLand",
//         };
//         ModestDbContext.Products.Add(entity);
//         await ModestDbContext.SaveChangesAsync();
//         // Act: delete once
//         var resp1 = await AlbaHost.Scenario(api =>
//         {
//             api.Delete.Url($"/api/references/products/{entity.Id}");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         // Act: delete again
//         var resp2 = await AlbaHost.Scenario(api =>
//         {
//             api.Delete.Url($"/api/references/products/{entity.Id}");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         var result = await resp2.ReadAsTextAsync();
//         result.Should().Be("false");
//     }
// }
