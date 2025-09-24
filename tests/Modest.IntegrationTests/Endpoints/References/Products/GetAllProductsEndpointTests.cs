using System.Net;
using FluentAssertions;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Products;

public class GetAllProductsEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task GetAllProductsReturnsAllAsync()
    {
        // Arrange: add 3 products
        var p1 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "A",
            Manufacturer = "M1",
            Country = "C1",
        };
        var p2 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "B",
            Manufacturer = "M2",
            Country = "C2",
        };
        var p3 = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "C",
            Manufacturer = "M3",
            Country = "C3",
        };
        ModestDbContext.Products.AddRange(p1, p2, p3);
        await ModestDbContext.SaveChangesAsync();
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
        // Arrange: add 15 products
        for (var i = 1; i <= 15; i++)
        {
            ModestDbContext.Products.Add(
                new ProductEntity
                {
                    Id = Guid.NewGuid(),
                    Name = $"Prod{i}",
                    Manufacturer = $"Man{i}",
                    Country = $"Land{i}",
                }
            );
        }

        await ModestDbContext.SaveChangesAsync();
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
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Alpha",
                Manufacturer = "A",
                Country = "X",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Beta",
                Manufacturer = "B",
                Country = "Y",
            }
        );
        await ModestDbContext.SaveChangesAsync();
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
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Alpha",
                Manufacturer = "A",
                Country = "X",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Beta",
                Manufacturer = "B",
                Country = "Y",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Gamma",
                Manufacturer = "A",
                Country = "Z",
            }
        );
        await ModestDbContext.SaveChangesAsync();
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
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Alpha",
                Manufacturer = "A",
                Country = "X",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Beta",
                Manufacturer = "B",
                Country = "Y",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Gamma",
                Manufacturer = "C",
                Country = "X",
            }
        );
        await ModestDbContext.SaveChangesAsync();
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
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "C",
                Manufacturer = "A",
                Country = "X",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "A",
                Manufacturer = "B",
                Country = "Y",
            }
        );
        ModestDbContext.Products.Add(
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "B",
                Manufacturer = "C",
                Country = "Z",
            }
        );
        await ModestDbContext.SaveChangesAsync();
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
