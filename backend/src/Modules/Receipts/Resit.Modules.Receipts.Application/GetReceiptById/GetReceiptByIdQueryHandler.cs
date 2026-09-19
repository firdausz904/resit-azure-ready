using Mediator;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Application.GetReceiptById;

public sealed class GetReceiptByIdQueryHandler(IReceiptRepository repository)
    : IRequestHandler<GetReceiptByIdQuery, ReceiptDetail?>
{
    public async ValueTask<ReceiptDetail?> Handle(GetReceiptByIdQuery query, CancellationToken cancellationToken)
    {
        var receipt = await repository.GetAsync(query.ReceiptId, query.HouseholdId, cancellationToken);

        return receipt is null
            ? null
            : new ReceiptDetail(
                receipt.Id,
                receipt.FileName,
                receipt.Merchant ?? "Unknown",
                receipt.Total ?? 0m,
                receipt.PurchasedOn,
                receipt.Category ?? "Uncategorized",
                receipt.Status.ToString(),
                receipt.MerchantConfidence,
                receipt.TotalConfidence,
                receipt.DateConfidence,
                receipt.RawOcrText);
    }
}
