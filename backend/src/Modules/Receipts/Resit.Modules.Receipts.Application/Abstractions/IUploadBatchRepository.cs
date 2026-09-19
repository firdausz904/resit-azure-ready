using Resit.Modules.Receipts.Domain;

namespace Resit.Modules.Receipts.Application.Abstractions;

public interface IUploadBatchRepository
{
    Task AddAsync(UploadBatch batch, CancellationToken cancellationToken);

    Task<UploadBatch?> GetAsync(Guid batchId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
