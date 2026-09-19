using Mediator;

namespace Resit.Modules.Bills.Application.DeleteBills;

public sealed record DeleteBillsCommand(Guid HouseholdId, IReadOnlyCollection<Guid> BillIds) : IRequest<int>;
