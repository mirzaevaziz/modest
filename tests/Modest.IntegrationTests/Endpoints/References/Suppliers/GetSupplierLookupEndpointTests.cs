using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Common.Models;
using Modest.Core.Features.References.Supplier;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Suppliers;

public class GetSupplierLookupEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task Given_NoSuppliers_When_GettingLookup_Then_ReturnsEmptyAsync()
    {
        // No suppliers in DB
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_FifteenSuppliers_When_GettingLookup_Then_ReturnsPagedResultsAsync()
    {
        // Add 15 suppliers using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        for (var i = 1; i <= 15; i++)
        {
            await supplierService.CreateSupplierAsync(
                new SupplierCreateDto($"Supplier{i}", null, null, null, null)
            );
        }

        // Page 1, size 10
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(10);
        result.TotalCount.Should().Be(15);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        // Validate SupplierLookupDto fields
        var first = result.Items[0];
        first.Id.Should().NotBeEmpty();
        first.Name.Should().NotBeNullOrEmpty();
        first.Code.Should().MatchRegex(@"^SUP-\d{6}$");
    }

    [Fact]
    public async Task Given_SearchFilter_When_GettingLookup_Then_ReturnsFilteredAsync()
    {
        // Add suppliers using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Alpha Corporation", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Beta Industries", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Gamma Solutions", null, null, null, null)
        );

        // Search for 'lph'
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?filter=lph&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
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
    public async Task Given_InvalidPaging_When_GettingLookup_Then_ReturnsBadRequestAsync(
        int page,
        int pageSize
    )
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?pageNumber={page}&pageSize={pageSize}");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_LargeDataset_When_GettingLookup_Then_ReturnsCorrectPaginationAsync()
    {
        // Add 100 suppliers using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        for (var i = 1; i <= 100; i++)
        {
            await supplierService.CreateSupplierAsync(
                new SupplierCreateDto($"Supplier{i:D3}", null, null, null, null)
            );
        }

        // Get page 5, size 20
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?pageNumber=5&pageSize=20");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(20);
        result.TotalCount.Should().Be(100);
        result.PageNumber.Should().Be(5);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Given_DeletedSupplier_When_GettingLookup_Then_ReturnsOnlyNonDeletedAsync()
    {
        // Add suppliers and delete one
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Active Supplier", null, null, null, null)
        );
        var deleted = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Deleted Supplier", null, null, null, null)
        );
        await supplierService.DeleteSupplierAsync(deleted.Id);

        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Name.Should().Be("Active Supplier");
    }

    [Fact]
    public async Task Given_Supplier_When_GettingLookup_Then_IncludesCodeAsync()
    {
        // Add a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var supplier = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Test Supplier", null, null, null, null)
        );

        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);

        // Validate all SupplierLookupDto fields
        var item = result.Items[0];
        item.Id.Should().Be(supplier.Id);
        item.Name.Should().Be("Test Supplier");
        item.Code.Should().MatchRegex(@"^SUP-\d{6}$");
        item.Code.Should().Be(supplier.Code);
    }

    [Fact]
    public async Task Given_MultiWordSearch_When_GettingLookup_Then_ReturnsMatchingAsync()
    {
        // Add suppliers using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Alpha Beta Corporation", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Beta Gamma Inc", null, null, null, null)
        );
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Delta Solutions", null, null, null, null)
        );

        // Search for suppliers with both "Alpha" and "Beta"
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?filter=Alpha+Beta&page=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Name.Should().Be("Alpha Beta Corporation");
    }

    [Fact]
    public async Task Given_CaseVariation_When_Searching_Then_IsCaseInsensitiveAsync()
    {
        // Add a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Test Supplier Corporation", null, null, null, null)
        );

        // Search with different casing
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/suppliers/lookup?filter=TEST+SUPPLIER&page=1&pageSize=10"
            );
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(1);
        result.Items[0].Name.Should().Be("Test Supplier Corporation");
    }

    [Fact]
    public async Task Given_PageBeyondTotal_When_GettingLookup_Then_ReturnsEmptyAsync()
    {
        // Arrange: add 5 suppliers
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        for (var i = 1; i <= 5; i++)
        {
            await supplierService.CreateSupplierAsync(
                new SupplierCreateDto($"Supplier{i}", null, null, null, null)
            );
        }

        // Act: request page 10 with size 10 (beyond total pages)
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/lookup?pageNumber=10&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<PaginatedResponse<SupplierLookupDto>>();
        result.Should().NotBeNull();
        // Items should be empty when requesting a page beyond available data
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(5); // At least our 5 suppliers exist
        result.PageNumber.Should().Be(10);
        result.PageSize.Should().Be(10);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    public async Task Given_InvalidPagination_When_GettingLookup_Then_ReturnsBadRequestAsync(
        int pageNumber,
        int pageSize
    )
    {
        // Act: request with invalid pagination parameters
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url(
                $"/api/references/suppliers/lookup?pageNumber={pageNumber}&pageSize={pageSize}"
            );
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }
}
