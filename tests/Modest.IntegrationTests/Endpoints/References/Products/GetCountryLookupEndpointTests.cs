// using System.Net;
// using FluentAssertions;
// using Modest.Core.Common.Models;
// using Modest.Core.Features.References.Product;
// using Xunit;

// namespace Modest.IntegrationTests.Endpoints.References.Products;

// public class GetCountryLookupEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
// {
//     [Fact]
//     public async Task GetCountryLookupReturnsEmptyAsync()
//     {
//         // No products in DB
//         var req = new PaginatedRequest<string>
//         {
//             Filter = null,
//             PageNumber = 1,
//             PageSize = 10,
//         };
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Get.Url($"/api/references/products/countries?page=1&pageSize=10");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         var result = await resp.ReadAsJsonAsync<PaginatedResponse<string>>();
//         result.Should().NotBeNull();
//         result!.Items.Should().BeEmpty();
//         result.TotalCount.Should().Be(0);
//     }

//     [Fact]
//     public async Task GetCountryLookupReturnsPagedDistinctResultsAsync()
//     {
//         // Add 15 products, 5 unique countries
//         var countries = new[] { "X", "Y", "Z", "W", "Q" };
//         for (var i = 1; i <= 15; i++)
//         {
//             ModestDbContext.Products.Add(
//                 new ProductEntity
//                 {
//                     Id = Guid.NewGuid(),
//                     Name = $"Prod{i}",
//                     Manufacturer = $"Man{i}",
//                     Country = countries[(i - 1) % 5],
//                 }
//             );
//         }

//         await ModestDbContext.SaveChangesAsync();
//         // Page 1, size 3
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Get.Url($"/api/references/products/countries?page=1&pageSize=3");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         var result = await resp.ReadAsJsonAsync<PaginatedResponse<string>>();
//         result.Should().NotBeNull();
//         result!.Items.Count.Should().Be(3);
//         result.TotalCount.Should().Be(5);
//         result.Items.Should().OnlyHaveUniqueItems();
//     }

//     [Fact]
//     public async Task GetCountryLookupWithSearchReturnsFilteredAsync()
//     {
//         // Add products
//         ModestDbContext.Products.Add(
//             new ProductEntity
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Alpha",
//                 Manufacturer = "A",
//                 Country = "AlphaLand",
//             }
//         );
//         ModestDbContext.Products.Add(
//             new ProductEntity
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Beta",
//                 Manufacturer = "B",
//                 Country = "BetaLand",
//             }
//         );
//         ModestDbContext.Products.Add(
//             new ProductEntity
//             {
//                 Id = Guid.NewGuid(),
//                 Name = "Gamma",
//                 Manufacturer = "C",
//                 Country = "GammaLand",
//             }
//         );
//         await ModestDbContext.SaveChangesAsync();
//         // Search for 'Beta'
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Get.Url($"/api/references/products/countries?filter=Beta&page=1&pageSize=10");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         var result = await resp.ReadAsJsonAsync<PaginatedResponse<string>>();
//         result.Should().NotBeNull();
//         result!.Items.Count.Should().Be(1);
//         result.Items[0].Should().Be("BetaLand");
//         result.TotalCount.Should().Be(1);
//     }

//     [Theory]
//     [InlineData(0, 10)]
//     [InlineData(1, 0)]
//     [InlineData(-1, 10)]
//     [InlineData(1, -5)]
//     public async Task GetCountryLookupInvalidPagingReturnsEmptyAsync(int page, int pageSize)
//     {
//         var resp = await AlbaHost.Scenario(api =>
//         {
//             api.Get.Url($"/api/references/products/countries?page={page}&pageSize={pageSize}");
//             api.StatusCodeShouldBe(HttpStatusCode.OK);
//         });
//         var result = await resp.ReadAsJsonAsync<PaginatedResponse<string>>();
//         result.Should().NotBeNull();
//         result!.Items.Should().BeEmpty();
//     }
// }
