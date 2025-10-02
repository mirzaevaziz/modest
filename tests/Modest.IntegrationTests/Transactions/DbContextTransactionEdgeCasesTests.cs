using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modest.Core.Data;
using Modest.Core.Features.References.Product;
using Xunit;

namespace Modest.IntegrationTests.Transactions;

public class DbContextTransactionEdgeCasesTests : IntegrationTestBase
{
    public DbContextTransactionEdgeCasesTests(WebFixture webFixture)
        : base(webFixture) { }

    [Fact]
    public async Task TransactionCommitPersistsChangesAsync()
    {
        using var transaction = await ModestDbContext.Database.BeginTransactionAsync();
        var product = new ProductEntity
        {
            Name = "T1",
            Manufacturer = "M1",
            Country = "C1",
        };
        ModestDbContext.Products.Add(product);
        await ModestDbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        var found = await ModestDbContext.Products.FirstOrDefaultAsync(x => x.Name == "T1");
        found.Should().NotBeNull();
    }

    [Fact]
    public async Task TransactionRollbackDoesNotPersistChangesAsync()
    {
        using var transaction = await ModestDbContext.Database.BeginTransactionAsync();
        var product = new ProductEntity
        {
            Name = "T2",
            Manufacturer = "M2",
            Country = "C2",
        };
        ModestDbContext.Products.Add(product);
        await ModestDbContext.SaveChangesAsync();
        await transaction.RollbackAsync();

        var found = await ModestDbContext.Products.FirstOrDefaultAsync(x => x.Name == "T2");
        found.Should().BeNull();
    }

    [Fact]
    public async Task TransactionExceptionRollsBackAsync()
    {
        var ex = await Record.ExceptionAsync(async () =>
        {
            using var transaction = await ModestDbContext.Database.BeginTransactionAsync();
            var product = new ProductEntity
            {
                Name = "T3",
                Manufacturer = "M3",
                Country = "C3",
            };
            ModestDbContext.Products.Add(product);
            await ModestDbContext.SaveChangesAsync();
            throw new InvalidOperationException("fail");
        });
        ex.Should().BeOfType<InvalidOperationException>();

        var found = await ModestDbContext.Products.FirstOrDefaultAsync(x => x.Name == "T3");
        found.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesOutsideTransactionPersistsChangesAsync()
    {
        var product = new ProductEntity
        {
            Name = "T4",
            Manufacturer = "M4",
            Country = "C4",
        };
        ModestDbContext.Products.Add(product);
        await ModestDbContext.SaveChangesAsync();

        var found = await ModestDbContext.Products.FirstOrDefaultAsync(x => x.Name == "T4");
        found.Should().NotBeNull();
    }
}
