using Mediator;

namespace Resit.Modules.Receipts.Application.GetReceipts;

public sealed record GetReceiptsQuery(
    Guid HouseholdId,
    string? Search,
    string? Category,
    DateOnly? From,
    DateOnly? To,
    int Page = 1,
    int PageSize = 20) : IRequest<GetReceiptsResult>;

public sealed record GetReceiptsResult(IReadOnlyList<ReceiptSummary> Items, int TotalCount);
