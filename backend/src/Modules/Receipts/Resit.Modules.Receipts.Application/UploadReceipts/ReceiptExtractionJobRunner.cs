using Hangfire;
using Resit.Modules.Notifications;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Application.UploadReceipts;

public sealed class ReceiptExtractionJobRunner(
    IReceiptRepository receipts,
    IUploadBatchRepository batches,
    IReceiptFileStorage fileStorage,
    IReceiptExtractionClient extractionClient,
    IRealtimeNotifier notifier)
{
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 5, 30, 120 })]
    public async Task RunAsync(Guid receiptId, Guid householdId, Guid batchId, CancellationToken cancellationToken)
    {
        var receipt = await receipts.GetAsync(receiptId, householdId, cancellationToken);
        if (receipt is null)
        {
            return;
        }

        receipt.MarkExtracting();
        await receipts.SaveChangesAsync(cancellationToken);

        try
        {
            await using var content = await fileStorage.OpenReadAsync(receipt.StoragePath, cancellationToken);
            var extracted = await extractionClient.ExtractAsync(content, receipt.FileName, cancellationToken);
            receipt.ApplyExtraction(extracted);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            receipt.MarkFailed(ex.Message);
        }

        await receipts.SaveChangesAsync(cancellationToken);

        var batch = await batches.GetAsync(batchId, cancellationToken);
        if (batch is null)
        {
            return;
        }

        if (receipt.Status == Domain.ReceiptStatus.Failed)
        {
            batch.RegisterFailure();
        }
        else
        {
            batch.RegisterSuccess();
        }

        await batches.SaveChangesAsync(cancellationToken);

        await notifier.NotifyBatchProgressAsync(
            householdId,
            new BatchProgressUpdate(batch.Id, batch.TotalCount, batch.ProcessedCount, batch.FailedCount),
            cancellationToken);

        await notifier.NotifyReceiptProcessedAsync(
            householdId,
            new ReceiptProcessedUpdate(
                batchId,
                receipt.Id,
                receipt.FileName,
                receipt.Status.ToString(),
                receipt.Merchant,
                receipt.Total,
                receipt.FailureReason),
            cancellationToken);
    }
}
