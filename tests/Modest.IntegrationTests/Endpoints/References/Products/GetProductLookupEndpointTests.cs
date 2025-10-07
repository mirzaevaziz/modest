using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class GetProductLookupEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task GetProductLookupReturnsEmptyAsync()
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
            api.Get.Url($"/api/references/products/lookup?page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetProductLookupReturnsPagedResultsAsync()
    {
        // Add 15 products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        for (var i = 1; i <= 15; i++)
        {
            await productService.CreateProductAsync(
                new ProductCreateDto($"Prod{i}", $"Man{i}", $"Land{i}", 1)
            );
        }

        // Page 1, size 10
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(10);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task GetProductLookupWithSearchReturnsFilteredAsync()
    {
        // Add products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("Alpha", "AAA", "XXXX", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Beta", "BAA", "YYYY", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Gamma", "CAA", "ZZZZ", 1));
        // Search for 'lph'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?filter=lph&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);

        result.Items[0].Name.Should().Contain("Alpha");

        result.TotalCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    [InlineData(1, -5)]
    public async Task GetProductLookupInvalidPagingReturnsEmptyAsync(int page, int pageSize)
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?page={page}&pageSize={pageSize}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductLookupWithLargeDatasetReturnsCorrectPaginationAsync()
    {
        // Add 100 products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        for (var i = 1; i <= 100; i++)
        {
            await productService.CreateProductAsync(
                new ProductCreateDto($"Product{i:D3}", $"Manufacturer{i}", $"Country{i}", i)
            );
        }

        // Test first page
        var resp1 = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?page=1&pageSize=20");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result1 = await resp1.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result1.Should().NotBeNull();
        result1!.Items.Count.Should().Be(20);
        result1.TotalCount.Should().Be(100);

        // Test last page
        var resp2 = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?page=5&pageSize=20");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result2 = await resp2.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result2.Should().NotBeNull();
        result2!.Items.Count.Should().Be(20);
        result2.TotalCount.Should().Be(100);
    }

    [Theory]
    [InlineData("ALPHA")]
    [InlineData("alpha")]
    [InlineData("AlPhA")]
    [InlineData("aLpHa")]
    public async Task GetProductLookupSearchIsCaseInsensitiveAsync(string filter)
    {
        // Add products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("Alpha", "AAA", "XXXX", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Beta", "BAA", "YYYY", 2));

        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?filter={filter}&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Name.Should().Contain("Alpha");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetProductLookupWithSpecialCharactersInFilterAsync()
    {
        // Add products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(
            new ProductCreateDto("Product-A", "Man-A", "Land-A", 1)
        );
        await productService.CreateProductAsync(
            new ProductCreateDto("Product_B", "Man_B", "Land_B", 2)
        );
        await productService.CreateProductAsync(
            new ProductCreateDto("Product.C", "Man.C", "Land.C", 3)
        );
        await productService.CreateProductAsync(
            new ProductCreateDto("Product D", "Man D", "Land D", 4)
        );

        // Search for hyphen
        var resp1 = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?filter=-&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result1 = await resp1.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result1.Should().NotBeNull();
        result1!.Items.Count.Should().BeGreaterThanOrEqualTo(1);

        // Search for underscore
        var resp2 = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?filter=_&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result2 = await resp2.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result2.Should().NotBeNull();
        result2!.Items.Count.Should().BeGreaterThanOrEqualTo(1);

        // Search for "Product D" - with word-by-word filter, this matches all products
        // containing both "Product" AND "D" (Product-A, Product_B, Product.C, Product D)
        var resp3 = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?filter=Product%20D&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result3 = await resp3.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result3.Should().NotBeNull();
        result3!.Items.Count.Should().Be(4); // All products have "Product" and "D" in FullName
        result3.Items.Should().Contain(p => p.Name.Contains("Product D")); // Specific one exists
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 5)]
    [InlineData(1, 50)]
    [InlineData(1, 100)]
    [InlineData(2, 50)]
    [InlineData(3, 50)]
    public async Task GetProductLookupValidPaginationEdgeCasesAsync(int page, int pageSize)
    {
        // Add products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        for (var i = 1; i <= 100; i++)
        {
            await productService.CreateProductAsync(
                new ProductCreateDto($"Prod{i:D3}", $"Man{i}", $"Land{i}", i % 100 + 1)
            );
        }

        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?page={page}&pageSize={pageSize}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result.Should().NotBeNull();
        result!.TotalCount.Should().Be(100);
    }

    [Fact]
    public async Task GetProductLookupWithNoMatchingFilterReturnsEmptyAsync()
    {
        // Add products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("Alpha", "AAA", "XXXX", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Beta", "BAA", "YYYY", 2));

        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?filter=NonExistent&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetProductLookupWithVeryLongFilterStringAsync()
    {
        // Add products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("Alpha", "AAA", "XXXX", 1));

        var longFilter = new string('x', 1000);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products/lookup?filter={longFilter}&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<PaginatedResponse<ProductLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
