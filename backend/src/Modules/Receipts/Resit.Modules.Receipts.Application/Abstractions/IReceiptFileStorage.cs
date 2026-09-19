namespace Resit.Modules.Receipts.Application.Abstractions;

public interface IReceiptFileStorage
{
    Task<string> SaveAsync(Guid householdId, string fileName, Stream content, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);
}
