namespace Resit.Modules.Receipts.Application.GetReceipts;

public sealed record ReceiptSummary(
    Guid Id,
    string Merchant,
    decimal Total,
    DateOnly? PurchasedOn,
    string Category,
    string Status,
    Guid UploadedByUserId);
