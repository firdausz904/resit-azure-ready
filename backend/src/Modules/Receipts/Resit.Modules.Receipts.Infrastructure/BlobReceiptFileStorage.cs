using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Infrastructure;

/// <summary>
/// Stores receipt files in Azure Blob Storage instead of local disk. Unlike local disk, this
/// survives App Service restarts, redeploys, and works across multiple instances if the app
/// is ever scaled out.
/// </summary>
public sealed class BlobReceiptFileStorage : IReceiptFileStorage
{
    private readonly BlobContainerClient _containerClient;

    public BlobReceiptFileStorage(IOptions<ReceiptStorageOptions> options)
    {
        var blobServiceClient = new BlobServiceClient(options.Value.BlobConnectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(options.Value.BlobContainerName);
    }

    public async Task<string> SaveAsync(Guid householdId, string fileName, Stream content, CancellationToken cancellationToken)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var storedFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var blobName = $"{householdId}/{storedFileName}";

        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, overwrite: true, cancellationToken);

        return blobName;
    }

    public async Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var blobClient = _containerClient.GetBlobClient(storagePath);
        var download = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        return download.Value.Content;
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        var blobClient = _containerClient.GetBlobClient(storagePath);
        return blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
