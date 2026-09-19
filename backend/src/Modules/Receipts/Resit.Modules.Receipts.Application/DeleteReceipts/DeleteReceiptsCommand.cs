using Mediator;

namespace Resit.Modules.Receipts.Application.DeleteReceipts;

public sealed record DeleteReceiptsCommand(Guid HouseholdId, IReadOnlyCollection<Guid> ReceiptIds) : IRequest<int>;
