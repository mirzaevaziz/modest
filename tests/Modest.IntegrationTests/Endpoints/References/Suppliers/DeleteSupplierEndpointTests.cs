using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Supplier;
using Modest.Data.Features.References.Supplier;
using MongoDB.Driver;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Suppliers;

public class DeleteSupplierEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Fact]
    public async Task Given_ValidSupplier_When_Deleting_Then_ReturnsOkAndSoftDeletesAsync()
    {
        // Arrange: create a supplier using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var mongoDatabase = AlbaHost.Services.GetRequiredService<IMongoDatabase>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Delete Me Supplier", null, null, null, null)
        );

        // Act: delete the supplier
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/suppliers/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsTextAsync();
        result.Should().Be("true");

        // Assert: supplier is soft deleted with all audit fields set
        var collection = mongoDatabase.GetCollection<SupplierEntity>("supplier");
        var inDb = await collection.Find(s => s.Id == entity.Id).FirstOrDefaultAsync();
        inDb.Should().NotBeNull();
        inDb!.IsDeleted.Should().BeTrue();
        inDb.DeletedAt.Should().NotBeNull();
        inDb.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        inDb.DeletedBy.Should().NotBeNullOrEmpty();
        inDb.UpdatedAt.Should().BeOnOrAfter(entity.UpdatedAt!.Value);
        inDb.CreatedAt.Should().Be(entity.CreatedAt); // CreatedAt should not change
    }

    [Fact]
    public async Task Given_NonExistentId_When_Deleting_Then_ReturnsOkFalseAsync()
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/suppliers/{Guid.NewGuid()}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsTextAsync();
        result.Should().Be("false");
    }

    [Fact]
    public async Task Given_AlreadyDeletedSupplier_When_DeletingAgain_Then_ReturnsOkFalseAsync()
    {
        // Arrange: create a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Delete Twice Test", null, null, null, null)
        );

        // Act: delete once
        var resp1 = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/suppliers/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result1 = await resp1.ReadAsTextAsync();
        result1.Should().Be("true");

        // Act: delete again
        var resp2 = await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/suppliers/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result2 = await resp2.ReadAsTextAsync();
        result2.Should().Be("false");
    }

    [Fact]
    public async Task Given_FullSupplierData_When_Deleting_Then_ValidatesAllDeletedFieldsAsync()
    {
        // Arrange: create a supplier with full data
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var mongoDatabase = AlbaHost.Services.GetRequiredService<IMongoDatabase>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Full Data", "John Doe", "+123", "test@example.com", "123 St")
        );
        var originalUpdatedAt = entity.UpdatedAt;

        // Act: delete the supplier
        await AlbaHost.Scenario(api =>
        {
            api.Delete.Url($"/api/references/suppliers/{entity.Id}");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Assert: all delete-related fields are properly set
        var collection = mongoDatabase.GetCollection<SupplierEntity>("supplier");
        var deleted = await collection.Find(s => s.Id == entity.Id).FirstOrDefaultAsync();
        deleted.Should().NotBeNull();
        deleted!.Id.Should().Be(entity.Id);
        deleted.Name.Should().Be("Full Data");
        deleted.Code.Should().Be(entity.Code);
        deleted.ContactPerson.Should().Be("John Doe");
        deleted.Phone.Should().Be("+123");
        deleted.Email.Should().Be("test@example.com");
        deleted.Address.Should().Be("123 St");
        deleted.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
        deleted.DeletedBy.Should().NotBeNullOrEmpty();
        deleted.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt!.Value);
        deleted.UpdatedBy.Should().NotBeNullOrEmpty();
        deleted.CreatedAt.Should().Be(entity.CreatedAt);
        deleted.CreatedBy.Should().Be(entity.CreatedBy);
    }

    [Fact]
    public async Task Given_DeletedSupplier_When_GettingAll_Then_NotReturnedAsync()
    {
        // Arrange: create and delete a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("To Be Hidden", null, null, null, null)
        );
        await supplierService.DeleteSupplierAsync(entity.Id);

        // Act: get all suppliers
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Get.Url("/api/references/suppliers?pageNumber=1&pageSize=10");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result =
            await resp.ReadAsJsonAsync<Modest.Core.Common.Models.PaginatedResponse<SupplierDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotContain(s => s.Id == entity.Id);
    }
}
