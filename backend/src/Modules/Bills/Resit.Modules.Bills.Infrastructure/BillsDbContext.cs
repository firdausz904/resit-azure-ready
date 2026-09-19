using Microsoft.EntityFrameworkCore;
using Resit.Modules.Bills.Domain;

namespace Resit.Modules.Bills.Infrastructure;

public sealed class BillsDbContext(DbContextOptions<BillsDbContext> options) : DbContext(options)
{
    public DbSet<Bill> Bills => Set<Bill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(builder =>
        {
            builder.ToTable("bills", "bills");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
            builder.Property(b => b.Category).HasMaxLength(100).IsRequired();
            builder.Property(b => b.Amount).HasColumnType("numeric(12,2)");
            builder.Property(b => b.Recurrence).HasConversion<string>().HasMaxLength(20);
            builder.HasIndex(b => new { b.HouseholdId, b.DueDate });
        });
    }
}
