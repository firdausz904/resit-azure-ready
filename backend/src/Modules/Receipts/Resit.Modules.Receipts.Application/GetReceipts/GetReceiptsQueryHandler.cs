using Mediator;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Application.GetReceipts;

public sealed class GetReceiptsQueryHandler(IReceiptRepository repository) : IRequestHandler<GetReceiptsQuery, GetReceiptsResult>
{
    public async ValueTask<GetReceiptsResult> Handle(GetReceiptsQuery query, CancellationToken cancellationToken)
    {
        var filter = new ReceiptFilter(query.Search, query.Category, query.From, query.To);

        var receipts = await repository.GetPagedAsync(query.HouseholdId, filter, query.Page, query.PageSize, cancellationToken);
        var totalCount = await repository.CountAsync(query.HouseholdId, filter, cancellationToken);

        var items = receipts
            .Select(r => new ReceiptSummary(
                r.Id,
                r.Merchant ?? "Unknown",
                r.Total ?? 0m,
                r.PurchasedOn,
                r.Category ?? "Uncategorized",
                r.Status.ToString(),
                r.UploadedByUserId))
            .ToList();

        return new GetReceiptsResult(items, totalCount);
    }
}
