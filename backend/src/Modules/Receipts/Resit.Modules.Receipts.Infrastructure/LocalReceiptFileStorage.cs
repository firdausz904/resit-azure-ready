using Microsoft.Extensions.Options;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Infrastructure;

public sealed class LocalReceiptFileStorage(IOptions<ReceiptStorageOptions> options) : IReceiptFileStorage
{
    public async Task<string> SaveAsync(Guid householdId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        var householdDirectory = Path.Combine(options.Value.RootPath, householdId.ToString());
        Directory.CreateDirectory(householdDirectory);

        var storedFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(householdDirectory, storedFileName);

        await using var destination = File.Create(fullPath);
        await content.CopyToAsync(destination, cancellationToken);

        return Path.Combine(householdId.ToString(), storedFileName);
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(options.Value.RootPath, storagePath);
        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        File.Delete(Path.Combine(options.Value.RootPath, storagePath));
        return Task.CompletedTask;
    }
}

public sealed class ReceiptStorageOptions
{
    public string RootPath { get; set; } = "storage/receipts";

    /// <summary>
    /// When set, receipt storage uses Azure Blob Storage (<see cref="BlobReceiptFileStorage"/>)
    /// instead of local disk. Leave unset for local development.
    /// </summary>
    public string? BlobConnectionString { get; set; }

    public string BlobContainerName { get; set; } = "receipts";
}
