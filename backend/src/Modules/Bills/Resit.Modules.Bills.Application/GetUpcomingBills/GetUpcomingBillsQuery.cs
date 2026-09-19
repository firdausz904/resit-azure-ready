using Mediator;

namespace Resit.Modules.Bills.Application.GetUpcomingBills;

public sealed record GetUpcomingBillsQuery(Guid HouseholdId) : IRequest<IReadOnlyList<UpcomingBill>>;
