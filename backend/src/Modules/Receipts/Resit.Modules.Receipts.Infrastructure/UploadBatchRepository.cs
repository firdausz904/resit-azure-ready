using Microsoft.EntityFrameworkCore;
using Resit.Modules.Receipts.Application.Abstractions;
using Resit.Modules.Receipts.Domain;

namespace Resit.Modules.Receipts.Infrastructure;

public sealed class UploadBatchRepository(ReceiptsDbContext dbContext) : IUploadBatchRepository
{
    public async Task AddAsync(UploadBatch batch, CancellationToken cancellationToken) =>
        await dbContext.UploadBatches.AddAsync(batch, cancellationToken);

    public Task<UploadBatch?> GetAsync(Guid batchId, CancellationToken cancellationToken) =>
        dbContext.UploadBatches.FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
