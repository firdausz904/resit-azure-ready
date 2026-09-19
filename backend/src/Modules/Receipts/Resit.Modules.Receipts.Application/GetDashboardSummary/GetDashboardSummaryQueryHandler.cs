using Mediator;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Application.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler(IReceiptRepository repository)
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummary>
{
    public async ValueTask<DashboardSummary> Handle(GetDashboardSummaryQuery query, CancellationToken cancellationToken)
    {
        var totals = await repository.GetTotalsAsync(query.HouseholdId, query.From, query.To, cancellationToken);

        var average = totals.ReceiptCount == 0
            ? 0m
            : Math.Round(totals.TotalSpent / totals.ReceiptCount, 2);

        return new DashboardSummary(totals.TotalSpent, totals.ReceiptCount, average, totals.ByCategory);
    }
}
