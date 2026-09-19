using Resit.Modules.Receipts.Domain;

namespace Resit.Modules.Receipts.Application.Abstractions;

public interface IReceiptRepository
{
    Task AddAsync(Receipt receipt, CancellationToken cancellationToken);

    void Remove(Receipt receipt);

    void RemoveRange(IEnumerable<Receipt> receipts);

    Task<IReadOnlyList<Receipt>> GetByIdsAsync(IReadOnlyCollection<Guid> receiptIds, Guid householdId, CancellationToken cancellationToken);

    Task<Receipt?> GetAsync(Guid receiptId, Guid householdId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Receipt>> GetPagedAsync(
        Guid householdId,
        ReceiptFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> CountAsync(Guid householdId, ReceiptFilter filter, CancellationToken cancellationToken);

    Task<ReceiptTotals> GetTotalsAsync(Guid householdId, DateOnly from, DateOnly to, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public readonly record struct ReceiptFilter(string? Search, string? Category, DateOnly? From, DateOnly? To);

public readonly record struct ReceiptTotals(decimal TotalSpent, int ReceiptCount, IReadOnlyDictionary<string, decimal> ByCategory);
