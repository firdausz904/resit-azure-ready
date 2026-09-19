using Mediator;

namespace Resit.Modules.Receipts.Application.DeleteReceipt;

public sealed record DeleteReceiptCommand(Guid HouseholdId, Guid ReceiptId) : IRequest<bool>;
