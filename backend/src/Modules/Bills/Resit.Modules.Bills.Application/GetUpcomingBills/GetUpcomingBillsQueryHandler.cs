using Mediator;
using Resit.Modules.Bills.Application.Abstractions;

namespace Resit.Modules.Bills.Application.GetUpcomingBills;

public sealed class GetUpcomingBillsQueryHandler(IBillRepository repository)
    : IRequestHandler<GetUpcomingBillsQuery, IReadOnlyList<UpcomingBill>>
{
    public async ValueTask<IReadOnlyList<UpcomingBill>> Handle(GetUpcomingBillsQuery query, CancellationToken cancellationToken)
    {
        var bills = await repository.GetUpcomingAsync(query.HouseholdId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return bills
            .OrderBy(b => b.DueDate)
            .Select(b => new UpcomingBill(
                b.Id,
                b.Name,
                b.Amount,
                b.DueDate,
                b.Recurrence.ToString(),
                b.Category,
                b.IsPaid,
                b.DueDate.DayNumber - today.DayNumber,
                b.NotifyDaysBefore))
            .ToList();
    }
}
