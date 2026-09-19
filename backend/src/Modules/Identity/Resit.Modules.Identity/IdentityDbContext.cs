using Microsoft.EntityFrameworkCore;

namespace Resit.Modules.Identity;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Household> Households => Set<Household>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        modelBuilder.Entity<Household>(builder =>
        {
            builder.ToTable("households");
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
            builder.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
            builder.Property(u => u.Initials).HasMaxLength(4).IsRequired();
            builder.HasIndex(u => u.Email).IsUnique();
        });
    }
}
