using Mediator;

namespace Resit.Modules.Receipts.Application.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(Guid HouseholdId, DateOnly From, DateOnly To) : IRequest<DashboardSummary>;

public sealed record DashboardSummary(
    decimal TotalSpent,
    int ReceiptCount,
    decimal AveragePerReceipt,
    IReadOnlyDictionary<string, decimal> SpendByCategory);
