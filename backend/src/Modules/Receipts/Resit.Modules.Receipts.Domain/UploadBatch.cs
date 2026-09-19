using Resit.SharedKernel;

namespace Resit.Modules.Receipts.Domain;

public sealed class UploadBatch : Entity
{
    public Guid HouseholdId { get; private init; }
    public int TotalCount { get; private set; }
    public int ProcessedCount { get; private set; }
    public int FailedCount { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }

    private UploadBatch() { }

    public static UploadBatch Create(Guid householdId, int totalCount) => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = householdId,
        TotalCount = totalCount,
        ProcessedCount = 0,
        FailedCount = 0,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public bool IsComplete => ProcessedCount + FailedCount >= TotalCount;

    public void RegisterSuccess() => ProcessedCount++;

    public void RegisterFailure()
    {
        ProcessedCount++;
        FailedCount++;
    }
}
