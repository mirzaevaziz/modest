using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Supplier;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Suppliers;

public class UpdateSupplierEndpointTests(WebFixture webFixture) : IntegrationTestBase(webFixture)
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    public async Task Given_InvalidName_When_UpdatingSupplier_Then_ReturnsBadRequestAsync(
        string? name
    )
    {
        // Arrange: create a valid supplier using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Valid Name", null, null, null, null)
        );

        // Act: try to update with invalid name
        var updateDto = new SupplierUpdateDto(entity.Id, name!, null, null, null, null);
        var updateResp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_ValidUpdate_When_UpdatingSupplier_Then_ReturnsOkAndUpdatesAsync()
    {
        // Arrange: create a valid supplier using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var supplierRepository = AlbaHost.Services.GetRequiredService<ISupplierRepository>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Original Name", "Old Contact", null, null, null)
        );

        // Act: update the supplier
        var updateDto = new SupplierUpdateDto(
            entity.Id,
            "Updated Name",
            "New Contact",
            "+9876543210",
            "updated@example.com",
            "456 New St"
        );
        var updateResp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var updated = await updateResp.ReadAsJsonAsync<SupplierDto>();
        updated.Should().NotBeNull();

        // Validate all fields
        updated!.Id.Should().Be(entity.Id);
        updated.Name.Should().Be("Updated Name");
        updated.ContactPerson.Should().Be("New Contact");
        updated.Phone.Should().Be("+9876543210");
        updated.Email.Should().Be("updated@example.com");
        updated.Address.Should().Be("456 New St");
        updated.Code.Should().Be(entity.Code); // Code should remain unchanged
        updated.IsDeleted.Should().BeFalse();
        updated.CreatedAt.Should().Be(entity.CreatedAt); // CreatedAt should not change
        updated.UpdatedAt.Should().NotBeNull();
        updated.UpdatedAt.Should().BeAfter(entity.UpdatedAt!.Value); // UpdatedAt should be newer
        updated.DeletedAt.Should().BeNull();
        updated.CreatedBy.Should().Be(entity.CreatedBy); // CreatedBy should not change
        updated.UpdatedBy.Should().NotBeNullOrEmpty();
        updated.DeletedBy.Should().BeNull();

        // Assert in DB
        var inDb = await supplierRepository.GetSupplierByIdAsync(entity.Id);
        inDb.Should().NotBeNull();
        inDb!.Name.Should().Be("Updated Name");
        inDb.ContactPerson.Should().Be("New Contact");
    }

    [Fact]
    public async Task Given_NonExistentId_When_UpdatingSupplier_Then_ReturnsNotFoundAsync()
    {
        var updateDto = new SupplierUpdateDto(Guid.NewGuid(), "Name", null, null, null, null);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task Given_DuplicateName_When_UpdatingSupplier_Then_ReturnsBadRequestAsync()
    {
        // Arrange: create two suppliers using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity1 = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Supplier One", null, null, null, null)
        );
        var entity2 = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Supplier Two", null, null, null, null)
        );

        // Act: try to update entity2 with entity1's name
        var updateDto = new SupplierUpdateDto(entity2.Id, "Supplier One", null, null, null, null);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_SameName_When_UpdatingSupplier_Then_ReturnsOkAsync()
    {
        // Arrange: create a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Same Name", "Contact", null, null, null)
        );

        // Act: update with same name but different contact
        var updateDto = new SupplierUpdateDto(
            entity.Id,
            "Same Name",
            "New Contact",
            null,
            null,
            null
        );
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var updated = await resp.ReadAsJsonAsync<SupplierDto>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Same Name");
        updated.ContactPerson.Should().Be("New Contact");
    }

    [Fact]
    public async Task Given_PartialFields_When_UpdatingSupplier_Then_OnlyUpdatesProvidedAsync()
    {
        // Arrange: create a supplier with all fields
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Original", "John Doe", "+123", "old@test.com", "Old Address")
        );

        // Act: update only name and phone, other fields should remain
        var updateDto = new SupplierUpdateDto(
            entity.Id,
            "New Name",
            entity.ContactPerson, // Keep same
            "+999",
            entity.Email, // Keep same
            entity.Address // Keep same
        );
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var updated = await resp.ReadAsJsonAsync<SupplierDto>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("New Name");
        updated.ContactPerson.Should().Be("John Doe"); // Unchanged
        updated.Phone.Should().Be("+999");
        updated.Email.Should().Be("old@test.com"); // Unchanged
        updated.Address.Should().Be("Old Address"); // Unchanged
    }

    [Fact]
    public async Task Given_ClearOptionalFields_When_UpdatingSupplier_Then_ReturnsOkAsync()
    {
        // Arrange: create a supplier with all fields
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Original", "John Doe", "+123", "old@test.com", "Old Address")
        );

        // Act: clear all optional fields
        var updateDto = new SupplierUpdateDto(entity.Id, "Updated Name", null, null, null, null);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var updated = await resp.ReadAsJsonAsync<SupplierDto>();
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Name");
        updated.ContactPerson.Should().BeNull();
        updated.Phone.Should().BeNull();
        updated.Email.Should().BeNull();
        updated.Address.Should().BeNull();
    }

    [Fact]
    public async Task Given_LongFields_When_UpdatingSupplier_Then_ReturnsBadRequestAsync()
    {
        // Arrange: create a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Original", null, null, null, null)
        );

        // Act: update with excessively long fields
        var updateDto = new SupplierUpdateDto(
            entity.Id,
            new string('A', 1001),
            new string('B', 1001),
            new string('C', 1001),
            new string('D', 1001),
            new string('E', 1001)
        );
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_DeletedSupplier_When_Updating_Then_ReturnsNotFoundAsync()
    {
        // Arrange: create and delete a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var entity = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("To Delete", null, null, null, null)
        );
        await supplierService.DeleteSupplierAsync(entity.Id);

        // Act: try to update the deleted supplier
        var updateDto = new SupplierUpdateDto(entity.Id, "New Name", null, null, null, null);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.NotFound);
        });
    }

    [Fact]
    public async Task Given_DeletedDuplicate_When_Updating_Then_ReturnsOkAndRenamesDeletedAsync()
    {
        // Arrange: create two suppliers using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var supplierRepository = AlbaHost.Services.GetRequiredService<ISupplierRepository>();
        var entity1 = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Supplier One", "Contact 1", "+111", "one@test.com", "Address 1")
        );
        var entity2 = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Supplier Two", "Contact 2", "+222", "two@test.com", "Address 2")
        );
        await supplierRepository.DeleteSupplierAsync(entity1.Id);

        // Act: Update entity2 to have the same name as deleted entity1
        var updateDto = new SupplierUpdateDto(
            entity2.Id,
            entity1.Name,
            "Updated Contact",
            "+999",
            "updated@test.com",
            "Updated Address"
        );
        var updateResp = await AlbaHost.Scenario(api =>
        {
            api.Put.Json(updateDto).ToUrl($"/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Assert: Deleted supplier (entity1) should be renamed with timestamp
        var deletedSupplier = await supplierRepository.GetSupplierByIdAsync(entity1.Id);
        deletedSupplier.Should().NotBeNull();
        deletedSupplier!.Name.Should().StartWith(entity1.Name + " - Changed ");
        deletedSupplier.Name.Should().NotBe(entity1.Name); // Name should be different due to timestamp
        deletedSupplier.Name.Length.Should().BeGreaterThan(entity1.Name.Length + 11); // " - Changed " + timestamp
        deletedSupplier.ContactPerson.Should().Be(entity1.ContactPerson);
        deletedSupplier.Phone.Should().Be(entity1.Phone);
        deletedSupplier.Email.Should().Be(entity1.Email);
        deletedSupplier.Address.Should().Be(entity1.Address);
        deletedSupplier.IsDeleted.Should().BeTrue(); // Should remain deleted
        deletedSupplier.DeletedAt.Should().NotBeNull();
        deletedSupplier.DeletedBy.Should().NotBeEmpty();

        // Assert: Updated supplier (entity2) should have the new name
        var updatedSupplier = await supplierRepository.GetSupplierByIdAsync(entity2.Id);
        updatedSupplier.Should().NotBeNull();
        updatedSupplier!.Name.Should().Be(entity1.Name);
        updatedSupplier.ContactPerson.Should().Be("Updated Contact");
        updatedSupplier.Phone.Should().Be("+999");
        updatedSupplier.Email.Should().Be("updated@test.com");
        updatedSupplier.Address.Should().Be("Updated Address");
        updatedSupplier.IsDeleted.Should().BeFalse();
        updatedSupplier.DeletedAt.Should().BeNull();
    }
}
