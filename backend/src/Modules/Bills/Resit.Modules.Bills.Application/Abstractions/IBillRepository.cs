using Resit.Modules.Bills.Domain;

namespace Resit.Modules.Bills.Application.Abstractions;

public interface IBillRepository
{
    Task AddAsync(Bill bill, CancellationToken cancellationToken);

    void Remove(Bill bill);

    void RemoveRange(IEnumerable<Bill> bills);

    Task<IReadOnlyList<Bill>> GetByIdsAsync(IReadOnlyCollection<Guid> billIds, Guid householdId, CancellationToken cancellationToken);

    Task<Bill?> GetAsync(Guid billId, Guid householdId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Bill>> GetUpcomingAsync(Guid householdId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Bill>> GetDueForNotificationAsync(DateOnly today, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetAllHouseholdIdsAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
