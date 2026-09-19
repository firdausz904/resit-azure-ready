using Mediator;

namespace Resit.Modules.Receipts.Application.EditReceipt;

public sealed record EditReceiptCommand(
    Guid HouseholdId,
    Guid ReceiptId,
    string Merchant,
    decimal Total,
    DateOnly PurchasedOn,
    string Category) : IRequest<bool>;
