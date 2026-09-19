using Microsoft.EntityFrameworkCore;
using Resit.Modules.Bills.Application.Abstractions;
using Resit.Modules.Bills.Domain;

namespace Resit.Modules.Bills.Infrastructure;

public sealed class BillRepository(BillsDbContext dbContext) : IBillRepository
{
    public async Task AddAsync(Bill bill, CancellationToken cancellationToken) =>
        await dbContext.Bills.AddAsync(bill, cancellationToken);

    public void Remove(Bill bill) => dbContext.Bills.Remove(bill);

    public void RemoveRange(IEnumerable<Bill> bills) => dbContext.Bills.RemoveRange(bills);

    public async Task<IReadOnlyList<Bill>> GetByIdsAsync(IReadOnlyCollection<Guid> billIds, Guid householdId, CancellationToken cancellationToken) =>
        await dbContext.Bills
            .Where(b => b.HouseholdId == householdId && billIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

    public Task<Bill?> GetAsync(Guid billId, Guid householdId, CancellationToken cancellationToken) =>
        dbContext.Bills.FirstOrDefaultAsync(b => b.Id == billId && b.HouseholdId == householdId, cancellationToken);

    public Task<IReadOnlyList<Bill>> GetUpcomingAsync(Guid householdId, CancellationToken cancellationToken) =>
        dbContext.Bills
            .Where(b => b.HouseholdId == householdId)
            .OrderBy(b => b.DueDate)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Bill>)t.Result, cancellationToken);

    public async Task<IReadOnlyList<Bill>> GetDueForNotificationAsync(DateOnly today, CancellationToken cancellationToken)
    {
        var candidates = await dbContext.Bills
            .Where(b => !b.IsPaid)
            .ToListAsync(cancellationToken);

        return candidates.Where(b => b.ShouldNotify(today)).ToList();
    }

    public Task<IReadOnlyList<Guid>> GetAllHouseholdIdsAsync(CancellationToken cancellationToken) =>
        dbContext.Bills
            .Select(b => b.HouseholdId)
            .Distinct()
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IReadOnlyList<Guid>)t.Result, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
