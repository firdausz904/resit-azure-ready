using Mediator;

namespace Resit.Modules.Receipts.Application.GetReceiptById;

public sealed record GetReceiptByIdQuery(Guid HouseholdId, Guid ReceiptId) : IRequest<ReceiptDetail?>;
