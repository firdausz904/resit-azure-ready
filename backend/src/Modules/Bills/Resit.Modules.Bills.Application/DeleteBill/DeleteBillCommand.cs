using Mediator;

namespace Resit.Modules.Bills.Application.DeleteBill;

public sealed record DeleteBillCommand(Guid HouseholdId, Guid BillId) : IRequest<bool>;
