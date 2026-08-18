using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Supplier;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Suppliers;

public class GetSupplierByIdEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task Given_ValidId_When_GettingSupplierById_Then_ReturnsSupplierAsync()
    {
        // Arrange: create a supplier using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Test Supplier", "John Doe", "+123456", "test@test.com", "123 St")
        );

        // Act
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<SupplierDto>();
        result.Should().NotBeNull();

        // Validate all fields
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be("Test Supplier");
        result.ContactPerson.Should().Be("John Doe");
        result.Phone.Should().Be("+123456");
        result.Email.Should().Be("test@test.com");
        result.Address.Should().Be("123 St");
        result.Code.Should().MatchRegex(@"^SUP-\d{6}$");
        result.IsDeleted.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.DeletedAt.Should().BeNull();
        result.CreatedBy.Should().NotBeNullOrEmpty();
        result.UpdatedBy.Should().NotBeNullOrEmpty();
        result.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task Given_InvalidId_When_GettingSupplierById_Then_ReturnsNotFoundAsync()
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/{Guid.NewGuid()}");
            api.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task Given_MinimalData_When_GettingSupplierById_Then_ReturnsSupplierAsync()
    {
        // Arrange: create a supplier with minimal data
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Minimal Supplier", null, null, null, null)
        );

        // Act
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<SupplierDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
        result.Name.Should().Be("Minimal Supplier");
        result.ContactPerson.Should().BeNull();
        result.Phone.Should().BeNull();
        result.Email.Should().BeNull();
        result.Address.Should().BeNull();
        result.Code.Should().MatchRegex(@"^SUP-\d{6}$");
        result.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task Given_DeletedSupplier_When_GettingById_Then_ReturnsWithDeletedFlagAsync()
    {
        // Arrange: create and delete a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Deleted Supplier", null, null, null, null)
        );
        await supplierService.DeleteSupplierAsync(entity.Id);

        // Act: deleted suppliers are still returned by GetById
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url($"/api/references/suppliers/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<SupplierDto>();
        result.Should().NotBeNull();
        result!.IsDeleted.Should().BeTrue();
        result.DeletedAt.Should().NotBeNull();
        result.DeletedBy.Should().NotBeNullOrEmpty();
    }
}
