using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class GetAllProductsEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task GetAllProductsReturnsAllAsync()
    {
        // Arrange: add 3 products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        var p1 = await productService.CreateProductAsync(
            new ProductCreateDto("AAAA", "M1111", "C1111", 1)
        );
        var p2 = await productService.CreateProductAsync(
            new ProductCreateDto("BBBB", "M2222", "C2222", 1)
        );
        var p3 = await productService.CreateProductAsync(
            new ProductCreateDto("CCCC", "M3333", "C3333", 1)
        );
        // Act
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(3);
        result.Select(x => x.Id).Should().Contain([p1.Id, p2.Id, p3.Id]);
    }

    [Fact]
    public async Task GetAllProductsReturnsPagedAsync()
    {
        // Arrange: add 15 products using the service
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        for (var i = 1; i <= 15; i++)
        {
            await productService.CreateProductAsync(
                new ProductCreateDto($"Prod{i}", $"Man{i}", $"Land{i}", 1)
            );
        }
        // Act: page 2, size 10
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products?pageNumber=2&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(5);
    }

    [Fact]
    public async Task GetAllProductsWithFilterReturnsFilteredAsync()
    {
        // Arrange
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("Alpha", "AAAA", "XXXX", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Beta", "BBBB", "YYYY", 1));
        // Act: filter by name 'lpha'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products?filter=%7B%0A%20%20%22searchText%22%3A%20%22lpha%22%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result[0].Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task GetAllProductsWithManufacturerFilterReturnsFilteredAsync()
    {
        // Arrange
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("Alpha", "AAAA", "XXXX", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Beta", "BBBB", "YYYY", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Gamma", "AAAA", "ZZZZ", 1));
        // Act: filter by manufacturer 'A'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products?filter=%7B%0A%20%20%22manufacturer%22%3A%20%22AAAA%22%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.All(x => x.Manufacturer == "AAAA").Should().BeTrue();
    }

    [Fact]
    public async Task GetAllProductsWithCountryFilterReturnsFilteredAsync()
    {
        // Arrange
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("Alpha", "AAAA", "XXXX", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Beta", "BBBB", "YYYY", 1));
        await productService.CreateProductAsync(new ProductCreateDto("Gamma", "CCCC", "XXXX", 1));
        // Act: filter by country 'X'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products?filter=%7B%0A%20%20%22country%22%3A%20%22XXXX%22%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.All(x => x.Country == "XXXX").Should().BeTrue();
    }

    [Fact]
    public async Task GetAllProductsWithSortReturnsSortedAsync()
    {
        // Arrange
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        await productService.CreateProductAsync(new ProductCreateDto("CCCC", "AAAA", "XXXX", 1));
        await productService.CreateProductAsync(new ProductCreateDto("AAAA", "BBBB", "YYYY", 1));
        await productService.CreateProductAsync(new ProductCreateDto("BBBB", "CCCC", "ZZZZ", 1));
        // Act: sort by name ascending
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products?sortFields=%7B%0A%20%20%22fieldName%22%3A%20%22Name%22%2C%0A%20%20%22ascending%22%3A%20true%0A%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(3);
        result.Select(x => x.Name).Should().ContainInOrder("AAAA", "BBBB", "CCCC");
    }

    [Fact]
    public async Task GetAllProductsReturnsEmptyIfNoneAsync()
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllProductsWithShowDeletedFalseReturnsOnlyActiveAsync()
    {
        // Arrange: create 3 products and delete one
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();

        var p1 = await productService.CreateProductAsync(
            new ProductCreateDto("Active1", "Man1", "Cou1", 1)
        );
        var p2 = await productService.CreateProductAsync(
            new ProductCreateDto("Active2", "Man2", "Cou2", 1)
        );
        var p3 = await productService.CreateProductAsync(
            new ProductCreateDto("Deleted1", "Man3", "Cou3", 1)
        );

        await productRepository.DeleteProductAsync(p3.Id);

        // Act: filter with ShowDeleted = false
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products?filter=%7B%0A%20%20%22showDeleted%22%3A%20false%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();

        // Assert: should return only active products
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.Select(x => x.Id).Should().Contain([p1.Id, p2.Id]);
        result.Select(x => x.Id).Should().NotContain(p3.Id);
    }

    [Fact]
    public async Task GetAllProductsWithShowDeletedTrueReturnsOnlyDeletedAsync()
    {
        // Arrange: create 3 products and delete one
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();

        var p1 = await productService.CreateProductAsync(
            new ProductCreateDto("Active1", "Man1", "Cou1", 1)
        );
        var p2 = await productService.CreateProductAsync(
            new ProductCreateDto("Active2", "Man2", "Cou2", 1)
        );
        var p3 = await productService.CreateProductAsync(
            new ProductCreateDto("Deleted1", "Man3", "Cou3", 1)
        );

        await productRepository.DeleteProductAsync(p3.Id);

        // Act: filter with ShowDeleted = true
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products?filter=%7B%0A%20%20%22showDeleted%22%3A%20true%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();

        // Assert: should return only deleted products
        result.Should().NotBeNull();
        result!.Count.Should().Be(1);
        result[0].Id.Should().Be(p3.Id);
        result.Select(x => x.Id).Should().NotContain([p1.Id, p2.Id]);
    }

    [Fact]
    public async Task GetAllProductsWithoutShowDeletedReturnsOnlyActiveByDefaultAsync()
    {
        // Arrange: create 3 products and delete one
        var productService = AlbaHost.Services.GetRequiredService<IProductService>();
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();

        var p1 = await productService.CreateProductAsync(
            new ProductCreateDto("Active1", "Man1", "Cou1", 1)
        );
        var p2 = await productService.CreateProductAsync(
            new ProductCreateDto("Active2", "Man2", "Cou2", 1)
        );
        var p3 = await productService.CreateProductAsync(
            new ProductCreateDto("Deleted1", "Man3", "Cou3", 1)
        );

        await productRepository.DeleteProductAsync(p3.Id);

        // Act: no ShowDeleted filter (should default to false)
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();

        // Assert: should return only active products by default
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.Select(x => x.Id).Should().Contain([p1.Id, p2.Id]);
        result.Select(x => x.Id).Should().NotContain(p3.Id);
    }
}
