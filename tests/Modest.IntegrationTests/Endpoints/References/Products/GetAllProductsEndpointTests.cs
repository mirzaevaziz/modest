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
        // Arrange: add 3 products using the repository
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        var p1 = await productRepository.CreateProductAsync(new ProductCreateDto("A", "M1", "C1"));
        var p2 = await productRepository.CreateProductAsync(new ProductCreateDto("B", "M2", "C2"));
        var p3 = await productRepository.CreateProductAsync(new ProductCreateDto("C", "M3", "C3"));
        // Act
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/products?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(3);
        result.Select(x => x.Id).Should().Contain(new[] { p1.Id, p2.Id, p3.Id });
    }

    [Fact]
    public async Task GetAllProductsReturnsPagedAsync()
    {
        // Arrange: add 15 products using the repository
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        for (var i = 1; i <= 15; i++)
        {
            await productRepository.CreateProductAsync(
                new ProductCreateDto($"Prod{i}", $"Man{i}", $"Land{i}")
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
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        await productRepository.CreateProductAsync(new ProductCreateDto("Alpha", "A", "X"));
        await productRepository.CreateProductAsync(new ProductCreateDto("Beta", "B", "Y"));
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
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        await productRepository.CreateProductAsync(new ProductCreateDto("Alpha", "A", "X"));
        await productRepository.CreateProductAsync(new ProductCreateDto("Beta", "B", "Y"));
        await productRepository.CreateProductAsync(new ProductCreateDto("Gamma", "A", "Z"));
        // Act: filter by manufacturer 'A'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products?filter=%7B%0A%20%20%22manufacturer%22%3A%20%22A%22%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.All(x => x.Manufacturer == "A").Should().BeTrue();
    }

    [Fact]
    public async Task GetAllProductsWithCountryFilterReturnsFilteredAsync()
    {
        // Arrange
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        await productRepository.CreateProductAsync(new ProductCreateDto("Alpha", "A", "X"));
        await productRepository.CreateProductAsync(new ProductCreateDto("Beta", "B", "Y"));
        await productRepository.CreateProductAsync(new ProductCreateDto("Gamma", "C", "X"));
        // Act: filter by country 'X'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/products?filter=%7B%0A%20%20%22country%22%3A%20%22X%22%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });
        var result = await resp.ReadAsJsonAsync<List<ProductDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
        result.All(x => x.Country == "X").Should().BeTrue();
    }

    [Fact]
    public async Task GetAllProductsWithSortReturnsSortedAsync()
    {
        // Arrange
        var productRepository = AlbaHost.Services.GetRequiredService<IProductRepository>();
        await productRepository.CreateProductAsync(new ProductCreateDto("C", "A", "X"));
        await productRepository.CreateProductAsync(new ProductCreateDto("A", "B", "Y"));
        await productRepository.CreateProductAsync(new ProductCreateDto("B", "C", "Z"));
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
        result.Select(x => x.Name).Should().ContainInOrder("A", "B", "C");
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
}
