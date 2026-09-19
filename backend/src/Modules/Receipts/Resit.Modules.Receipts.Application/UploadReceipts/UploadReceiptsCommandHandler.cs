using Hangfire;
using Mediator;
using Resit.Modules.Notifications;
using Resit.Modules.Receipts.Application.Abstractions;
using Resit.Modules.Receipts.Domain;

namespace Resit.Modules.Receipts.Application.UploadReceipts;

public sealed class UploadReceiptsCommandHandler(
    IUploadBatchRepository batches,
    IReceiptRepository receipts,
    IReceiptFileStorage fileStorage,
    IBackgroundJobClient backgroundJobs,
    IRealtimeNotifier notifier) : IRequestHandler<UploadReceiptsCommand, UploadReceiptsResult>
{
    public async ValueTask<UploadReceiptsResult> Handle(UploadReceiptsCommand command, CancellationToken cancellationToken)
    {
        var batch = UploadBatch.Create(command.HouseholdId, command.Files.Count);
        await batches.AddAsync(batch, cancellationToken);

        var queuedReceiptIds = new List<Guid>(command.Files.Count);

        foreach (var file in command.Files)
        {
            var storagePath = await fileStorage.SaveAsync(command.HouseholdId, file.FileName, file.Content, cancellationToken);

            var receipt = Receipt.CreateQueued(
                command.HouseholdId,
                command.UploadedByUserId,
                batch.Id,
                file.FileName,
                storagePath);

            await receipts.AddAsync(receipt, cancellationToken);
            queuedReceiptIds.Add(receipt.Id);
        }

        await batches.SaveChangesAsync(cancellationToken);
        await receipts.SaveChangesAsync(cancellationToken);

        foreach (var receiptId in queuedReceiptIds)
        {
            backgroundJobs.Enqueue<ReceiptExtractionJobRunner>(
                runner => runner.RunAsync(receiptId, command.HouseholdId, batch.Id, CancellationToken.None));
        }

        await notifier.NotifyBatchProgressAsync(
            command.HouseholdId,
            new BatchProgressUpdate(batch.Id, batch.TotalCount, 0, 0),
            cancellationToken);

        return new UploadReceiptsResult(batch.Id, batch.TotalCount);
    }
}
