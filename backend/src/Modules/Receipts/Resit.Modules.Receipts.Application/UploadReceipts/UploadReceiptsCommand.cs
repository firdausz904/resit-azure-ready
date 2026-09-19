using Mediator;

namespace Resit.Modules.Receipts.Application.UploadReceipts;

public sealed record UploadReceiptsCommand(
    Guid HouseholdId,
    Guid UploadedByUserId,
    IReadOnlyList<UploadedFile> Files) : IRequest<UploadReceiptsResult>;

public sealed record UploadReceiptsResult(Guid BatchId, int QueuedCount);
