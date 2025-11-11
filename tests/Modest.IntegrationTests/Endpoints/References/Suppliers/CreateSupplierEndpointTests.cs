using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Modest.Core.Features.References.Supplier;
using Xunit;

namespace Modest.IntegrationTests.Endpoints.References.Suppliers;

public class CreateSupplierEndpointTests(WebFixture mongoDbFixture)
    : IntegrationTestBase(mongoDbFixture)
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    public async Task Given_InvalidName_When_CreatingSupplier_Then_ReturnsBadRequestAsync(
        string? name
    )
    {
        var dto = new SupplierCreateDto(name!, null, null, null, null);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_LongName_When_CreatingSupplier_Then_ReturnsBadRequestAsync()
    {
        var longStr = new string('a', 1001);
        var dto = new SupplierCreateDto(longStr, null, null, null, null);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_NullDto_When_CreatingSupplier_Then_ReturnsBadRequestAsync()
    {
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(new { }).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_ValidData_When_CreatingSupplier_Then_ReturnsOkAndSupplierAsync()
    {
        // Arrange
        var dto = new SupplierCreateDto(
            "Test Supplier",
            "John Doe",
            "+1234567890",
            "test@example.com",
            "123 Test St"
        );

        // Act & Assert using Alba
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Assert - Validate all fields
        var result = await resp.ReadAsJsonAsync<SupplierDto>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Supplier");
        result.ContactPerson.Should().Be("John Doe");
        result.Phone.Should().Be("+1234567890");
        result.Email.Should().Be("test@example.com");
        result.Address.Should().Be("123 Test St");
        result.Code.Should().StartWith("SUP-");
        result.Code.Should().MatchRegex(@"^SUP-\d{6}$");
        result.IsDeleted.Should().BeFalse();
        result.CreatedAt.Should().NotBeNull();
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.DeletedAt.Should().BeNull();
        result.CreatedBy.Should().NotBeNullOrEmpty();
        result.UpdatedBy.Should().NotBeNullOrEmpty();
        result.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task Given_MinimalData_When_CreatingSupplier_Then_ReturnsOkAsync()
    {
        // Arrange - only required field
        var dto = new SupplierCreateDto("Minimal Supplier", null, null, null, null);

        // Act
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Assert - Validate all fields
        var result = await resp.ReadAsJsonAsync<SupplierDto>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result!.Name.Should().Be("Minimal Supplier");
        result.ContactPerson.Should().BeNull();
        result.Phone.Should().BeNull();
        result.Email.Should().BeNull();
        result.Address.Should().BeNull();
        result.Code.Should().StartWith("SUP-");
        result.IsDeleted.Should().BeFalse();
        result.CreatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().NotBeNull();
        result.DeletedAt.Should().BeNull();
        result.CreatedBy.Should().NotBeNullOrEmpty();
        result.UpdatedBy.Should().NotBeNullOrEmpty();
        result.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task Given_DuplicateName_When_CreatingSupplier_Then_ReturnsBadRequestAsync()
    {
        // Arrange: create a supplier using the service
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Duplicate Name", null, null, null, null)
        );

        // Act: try to create with same name
        var dto = new SupplierCreateDto("Duplicate Name", null, null, null, null);
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_DeletedSupplier_When_CreatingWithSameName_Then_RestoresSupplierAsync()
    {
        // Arrange: create and delete a supplier
        var supplierService = AlbaHost.Services.GetRequiredService<ISupplierService>();
        var created = await supplierService.CreateSupplierAsync(
            new SupplierCreateDto("Restore Test", "Old Contact", null, null, null)
        );
        await supplierService.DeleteSupplierAsync(created.Id);

        // Act: create with same name
        var dto = new SupplierCreateDto(
            "Restore Test",
            "New Contact",
            "+987654",
            "new@test.com",
            "New Address"
        );
        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        // Assert: should restore and update - validate all fields
        var result = await resp.ReadAsJsonAsync<SupplierDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id); // Same ID
        result.Name.Should().Be("Restore Test");
        result.ContactPerson.Should().Be("New Contact");
        result.Phone.Should().Be("+987654");
        result.Email.Should().Be("new@test.com");
        result.Address.Should().Be("New Address");
        result.IsDeleted.Should().BeFalse();
        result.Code.Should().Be(created.Code); // Code should remain the same
        result.DeletedAt.Should().BeNull();
        result.DeletedBy.Should().BeNull();
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt.Should().BeAfter(created.UpdatedAt!.Value);
    }

    [Fact]
    public async Task Given_LongFields_When_CreatingSupplier_Then_ReturnsBadRequestAsync()
    {
        var longStr = new string('a', 1001);

        // Long ContactPerson
        var dto1 = new SupplierCreateDto("Valid Name", longStr, null, null, null);
        await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto1).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        // Long Phone
        var dto2 = new SupplierCreateDto("Valid Name", null, longStr, null, null);
        await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto2).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        // Long Email
        var dto3 = new SupplierCreateDto("Valid Name", null, null, longStr, null);
        await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto3).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });

        // Long Address
        var dto4 = new SupplierCreateDto("Valid Name", null, null, null, longStr);
        await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto4).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.BadRequest);
        });
    }

    [Fact]
    public async Task Given_SpecialCharacters_When_CreatingSupplier_Then_ReturnsOkAsync()
    {
        var dto = new SupplierCreateDto(
            "Supplier & Co., Ltd.",
            "O'Brien",
            "+1 (555) 123-4567",
            "test+tag@example.co.uk",
            "123 Main St., Apt. #4B"
        );

        var resp = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result = await resp.ReadAsJsonAsync<SupplierDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Supplier & Co., Ltd.");
        result.ContactPerson.Should().Be("O'Brien");
        result.Phone.Should().Be("+1 (555) 123-4567");
        result.Email.Should().Be("test+tag@example.co.uk");
        result.Address.Should().Be("123 Main St., Apt. #4B");
    }

    [Fact]
    public async Task Given_MultipleSuppliers_When_Creating_Then_EachHasUniqueCodeAsync()
    {
        var dto1 = new SupplierCreateDto("Supplier One", null, null, null, null);
        var dto2 = new SupplierCreateDto("Supplier Two", null, null, null, null);

        var resp1 = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto1).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var resp2 = await AlbaHost.Scenario(api =>
        {
            api.Post.Json(dto2).ToUrl("/api/references/suppliers");
            api.StatusCodeShouldBe(HttpStatusCode.OK);
        });

        var result1 = await resp1.ReadAsJsonAsync<SupplierDto>();
        var result2 = await resp2.ReadAsJsonAsync<SupplierDto>();

        result1!.Code.Should().NotBe(result2!.Code);
        result1.Code.Should().MatchRegex(@"^SUP-\d{6}$");
        result2.Code.Should().MatchRegex(@"^SUP-\d{6}$");
    }
}
