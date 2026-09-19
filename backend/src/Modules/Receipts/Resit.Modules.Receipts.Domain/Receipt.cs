using Resit.SharedKernel;

namespace Resit.Modules.Receipts.Domain;

public sealed class Receipt : Entity
{
    public Guid HouseholdId { get; private init; }
    public Guid UploadedByUserId { get; private init; }
    public Guid? BatchId { get; private init; }
    public string FileName { get; private init; } = string.Empty;
    public string StoragePath { get; private init; } = string.Empty;
    public ReceiptStatus Status { get; private set; } = ReceiptStatus.Queued;
    public string? Merchant { get; private set; }
    public decimal? Total { get; private set; }
    public DateOnly? PurchasedOn { get; private set; }
    public string? Category { get; private set; }
    public double? MerchantConfidence { get; private set; }
    public double? TotalConfidence { get; private set; }
    public double? DateConfidence { get; private set; }
    public string? RawOcrText { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Receipt() { }

    public static Receipt CreateQueued(
        Guid householdId,
        Guid uploadedByUserId,
        Guid? batchId,
        string fileName,
        string storagePath)
    {
        var now = DateTimeOffset.UtcNow;

        return new Receipt
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            UploadedByUserId = uploadedByUserId,
            BatchId = batchId,
            FileName = fileName,
            StoragePath = storagePath,
            Status = ReceiptStatus.Queued,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void MarkExtracting() => Touch(ReceiptStatus.Extracting);

    public void ApplyExtraction(ExtractedReceiptData data)
    {
        Merchant = data.Merchant;
        Total = data.Total;
        PurchasedOn = data.PurchasedOn;
        Category = data.SuggestedCategory;
        MerchantConfidence = data.MerchantConfidence;
        TotalConfidence = data.TotalConfidence;
        DateConfidence = data.DateConfidence;
        RawOcrText = data.RawText;

        var lowestConfidence = new[] { data.MerchantConfidence, data.TotalConfidence, data.DateConfidence }.Min();
        Touch(lowestConfidence < 0.85 ? ReceiptStatus.NeedsReview : ReceiptStatus.Done);
    }

    public void MarkFailed(string reason)
    {
        FailureReason = reason;
        Touch(ReceiptStatus.Failed);
    }

    public void Edit(string merchant, decimal total, DateOnly purchasedOn, string category)
    {
        Merchant = merchant;
        Total = total;
        PurchasedOn = purchasedOn;
        Category = category;
        Touch(ReceiptStatus.Done);
    }

    private void Touch(ReceiptStatus status)
    {
        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public readonly record struct ExtractedReceiptData(
    string Merchant,
    decimal Total,
    DateOnly PurchasedOn,
    string SuggestedCategory,
    double MerchantConfidence,
    double TotalConfidence,
    double DateConfidence,
    string RawText);
