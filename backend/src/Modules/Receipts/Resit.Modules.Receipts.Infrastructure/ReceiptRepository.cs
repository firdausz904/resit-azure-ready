using Microsoft.EntityFrameworkCore;
using Resit.Modules.Receipts.Application.Abstractions;
using Resit.Modules.Receipts.Domain;

namespace Resit.Modules.Receipts.Infrastructure;

public sealed class ReceiptRepository(ReceiptsDbContext dbContext) : IReceiptRepository
{
    public async Task AddAsync(Receipt receipt, CancellationToken cancellationToken) =>
        await dbContext.Receipts.AddAsync(receipt, cancellationToken);

    public void Remove(Receipt receipt) => dbContext.Receipts.Remove(receipt);

    public void RemoveRange(IEnumerable<Receipt> receipts) => dbContext.Receipts.RemoveRange(receipts);

    public async Task<IReadOnlyList<Receipt>> GetByIdsAsync(IReadOnlyCollection<Guid> receiptIds, Guid householdId, CancellationToken cancellationToken) =>
        await dbContext.Receipts
            .Where(r => r.HouseholdId == householdId && receiptIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

    public Task<Receipt?> GetAsync(Guid receiptId, Guid householdId, CancellationToken cancellationToken) =>
        dbContext.Receipts.FirstOrDefaultAsync(r => r.Id == receiptId && r.HouseholdId == householdId, cancellationToken);

    public async Task<IReadOnlyList<Receipt>> GetPagedAsync(
        Guid householdId,
        ReceiptFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilter(dbContext.Receipts.Where(r => r.HouseholdId == householdId), filter);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(Guid householdId, ReceiptFilter filter, CancellationToken cancellationToken) =>
        ApplyFilter(dbContext.Receipts.Where(r => r.HouseholdId == householdId), filter).CountAsync(cancellationToken);

    public async Task<ReceiptTotals> GetTotalsAsync(Guid householdId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var receipts = await dbContext.Receipts
            .Where(r => r.HouseholdId == householdId &&
                        r.PurchasedOn != null &&
                        r.PurchasedOn >= from &&
                        r.PurchasedOn <= to &&
                        r.Status != ReceiptStatus.Failed)
            .ToListAsync(cancellationToken);

        var totalSpent = receipts.Sum(r => r.Total ?? 0m);
        var byCategory = receipts
            .GroupBy(r => r.Category ?? "Uncategorized")
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Total ?? 0m));

        return new ReceiptTotals(totalSpent, receipts.Count, byCategory);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);

    private static IQueryable<Receipt> ApplyFilter(IQueryable<Receipt> query, ReceiptFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(r => r.Merchant != null && r.Merchant.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(r => r.Category == filter.Category);
        }

        if (filter.From is not null)
        {
            query = query.Where(r => r.PurchasedOn >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(r => r.PurchasedOn <= filter.To);
        }

        return query;
    }
}
