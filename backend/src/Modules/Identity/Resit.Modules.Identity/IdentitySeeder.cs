using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Resit.Modules.Identity;

public static class IdentitySeeder
{
    // EnsureCreated skips table creation when the database already holds any table (e.g. Hangfire's),
    // so create the identity tables explicitly.
    public static async Task EnsureCreatedAsync(IdentityDbContext dbContext, CancellationToken cancellationToken)
    {
        var creator = dbContext.GetService<IRelationalDatabaseCreator>();

        if (!await creator.ExistsAsync(cancellationToken))
        {
            await creator.CreateAsync(cancellationToken);
        }

        try
        {
            await creator.CreateTablesAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.DuplicateTable)
        {
        }
    }

    public static async Task SeedDevelopmentDataAsync(IdentityDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var household = Household.Create("The Firdaus Family");
        await dbContext.Households.AddAsync(household, cancellationToken);

        var user = User.Create(household.Id, "firdausz904@gmail.com", "Firdaus Zakaria", "FZ");
        user.SetPasswordHash(PasswordHasher.Hash("123456"));
        await dbContext.Users.AddAsync(user, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
