namespace Resit.Modules.Receipts.Application.GetReceiptById;

public sealed record ReceiptDetail(
    Guid Id,
    string FileName,
    string Merchant,
    decimal Total,
    DateOnly? PurchasedOn,
    string Category,
    string Status,
    double? MerchantConfidence,
    double? TotalConfidence,
    double? DateConfidence,
    string? RawOcrText);
