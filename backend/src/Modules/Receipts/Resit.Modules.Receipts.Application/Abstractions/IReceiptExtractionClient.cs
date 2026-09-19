using Resit.Modules.Receipts.Domain;

namespace Resit.Modules.Receipts.Application.Abstractions;

public interface IReceiptExtractionClient
{
    Task<ExtractedReceiptData> ExtractAsync(Stream fileContent, string fileName, CancellationToken cancellationToken);
}
