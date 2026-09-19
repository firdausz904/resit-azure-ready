namespace Resit.Modules.Receipts.Domain;

public enum ReceiptStatus
{
    Queued,
    Extracting,
    NeedsReview,
    Done,
    Failed
}
