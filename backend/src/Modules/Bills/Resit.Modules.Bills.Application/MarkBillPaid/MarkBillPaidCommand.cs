using Mediator;

namespace Resit.Modules.Bills.Application.MarkBillPaid;

public sealed record MarkBillPaidCommand(Guid HouseholdId, Guid BillId) : IRequest<bool>;
