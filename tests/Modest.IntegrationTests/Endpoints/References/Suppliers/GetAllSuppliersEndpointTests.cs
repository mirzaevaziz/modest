using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Supplier;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Suppliers;

public class GetAllSuppliersEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task Given_ThreeSuppliers_When_GettingAll_Then_ReturnsAllAsync()
    {
        // Arrange: add 3 suppliers using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var s1 = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Supplier A", "Contact A", "+111", "a@test.com", "Address A")
        );
        var s2 = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Supplier B", "Contact B", "+222", "b@test.com", "Address B")
        );
        var s3 = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Supplier C", "Contact C", "+333", "c@test.com", "Address C")
        );

        // Act
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(3);
        result.Items.Select(x => x.Id).Should().Contain([s1.Id, s2.Id, s3.Id]);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        // Validate all fields of first item
        var first = result.Items.First();
        first.Id.Should().NotBeEmpty();
        first.Name.Should().NotBeNullOrEmpty();
        first.IsDeleted.Should().BeFalse();
        first.CreatedAt.Should().NotBeNull();
        first.UpdatedAt.Should().NotBeNull();
        first.DeletedAt.Should().BeNull();
        first.CreatedBy.Should().NotBeNullOrEmpty();
        first.UpdatedBy.Should().NotBeNullOrEmpty();
        first.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task Given_FifteenSuppliers_When_GettingPageTwo_Then_ReturnsPagedAsync()
    {
        // Arrange: add 15 suppliers using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        for (var i = 1; i <= 15; i++)
        {
            await supplierService.CreateSupplierAsync(
                new SupplierCreateDto($"Supplier{i}", $"Contact{i}", null, null, null)
            );
        }

        // Act: page 2, size 10
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers?pageNumber=2&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(5);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Given_FilterBySearchText_When_GettingAll_Then_ReturnsFilteredAsync()
    {
        // Arrange
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Alpha Corp", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Beta Inc", null, null, null, null)
        );

        // Act: filter by name 'lpha'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/suppliers?filter=%7B%0A%20%20%22searchText%22%3A%20%22lpha%22%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Name.Should().Be("Alpha Corp");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Given_NoSuppliers_When_GettingAll_Then_ReturnsEmptyListAsync()
    {
        // Act: No suppliers in DB
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_SortByNameAscending_When_GettingAll_Then_ReturnsOrderedAsync()
    {
        // Arrange
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Zebra Supplier", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Alpha Supplier", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Beta Supplier", null, null, null, null)
        );

        // Act: sort by Name ascending
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/suppliers?sortFields=%5B%7B%22fieldName%22%3A%22Name%22%2C%22ascending%22%3Atrue%7D%5D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(3);
        result.Items[0].Name.Should().Be("Alpha Supplier");
        result.Items[1].Name.Should().Be("Beta Supplier");
        result.Items[2].Name.Should().Be("Zebra Supplier");
    }

    [Fact]
    public async Task Given_DeletedSupplier_When_GettingAll_Then_ExcludesDeletedByDefaultAsync()
    {
        // Arrange
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Active Supplier", null, null, null, null)
        );
        var deleted = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Deleted Supplier", null, null, null, null)
        );
        await supplierService.DeleteSupplierAsync(deleted.Id);

        // Act
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Name.Should().Be("Active Supplier");
        result.Items[0].IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Given_ShowDeletedFilter_When_GettingAll_Then_ReturnsDeletedAsync()
    {
        // Arrange
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Active Supplier", null, null, null, null)
        );
        var deleted = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Deleted Supplier", null, null, null, null)
        );
        await supplierService.DeleteSupplierAsync(deleted.Id);

        // Act: filter with showDeleted=true
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/suppliers?filter=%7B%22showDeleted%22%3Atrue%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Name.Should().Be("Deleted Supplier");
        result.Items[0].IsDeleted.Should().BeTrue();
        result.Items[0].DeletedAt.Should().NotBeNull();
        result.Items[0].DeletedBy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Given_MultiWordSearch_When_GettingAll_Then_ReturnsMatchingAsync()
    {
        // Arrange
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Alpha Beta Gamma", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Beta Delta", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Gamma Epsilon", null, null, null, null)
        );

        // Act: search for suppliers containing both "Alpha" and "Beta"
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/suppliers?filter=%7B%22searchText%22%3A%22Alpha%20Beta%22%7D&pageNumber=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Name.Should().Be("Alpha Beta Gamma");
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public async Task Given_InvalidPagination_When_GettingAll_Then_ReturnsBadRequestAsync(
        int pageNumber,
        int pageSize
    )
    {
        // Act: request with invalid pagination parameters
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers?pageNumber={pageNumber}&pageSize={pageSize}");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }
}
