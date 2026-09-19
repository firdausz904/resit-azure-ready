using Microsoft.EntityFrameworkCore;
using Resit.Modules.Receipts.Domain;

namespace Resit.Modules.Receipts.Infrastructure;

public sealed class ReceiptsDbContext(DbContextOptions<ReceiptsDbContext> options) : DbContext(options)
{
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<UploadBatch> UploadBatches => Set<UploadBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Receipt>(builder =>
        {
            builder.ToTable("receipts", "receipts");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.FileName).HasMaxLength(260).IsRequired();
            builder.Property(r => r.StoragePath).HasMaxLength(500).IsRequired();
            builder.Property(r => r.Merchant).HasMaxLength(200);
            builder.Property(r => r.Category).HasMaxLength(100);
            builder.Property(r => r.Total).HasColumnType("numeric(12,2)");
            builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            builder.HasIndex(r => new { r.HouseholdId, r.PurchasedOn });
            builder.HasIndex(r => r.BatchId);
        });

        modelBuilder.Entity<UploadBatch>(builder =>
        {
            builder.ToTable("upload_batches", "receipts");
            builder.HasKey(b => b.Id);
        });
    }
}
