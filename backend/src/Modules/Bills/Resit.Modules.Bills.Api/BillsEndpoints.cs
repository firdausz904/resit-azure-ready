using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Resit.Modules.Bills.Application.CreateBillReminder;
using Resit.Modules.Bills.Application.DeleteBill;
using Resit.Modules.Bills.Application.DeleteBills;
using Resit.Modules.Bills.Application.GetUpcomingBills;
using Resit.Modules.Bills.Application.MarkBillPaid;
using Resit.Modules.Bills.Application.UpdateBillReminder;
using Resit.Modules.Bills.Domain;
using Resit.Modules.Identity;

namespace Resit.Modules.Bills.Api;

public static class BillsEndpoints
{
    private const int MaxBulkDeleteCount = 100;

    public static IEndpointRouteBuilder MapBillsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/households/{householdId:guid}/bills").WithTags("Bills").RequireHouseholdMatch();

        group.MapGet("/", async (Guid householdId, ISender sender, CancellationToken cancellationToken) =>
        {
            var bills = await sender.Send(new GetUpcomingBillsQuery(householdId), cancellationToken);
            return Results.Ok(bills);
        });

        group.MapPost("/", async (Guid householdId, CreateBillRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new CreateBillReminderCommand(
                householdId,
                request.CreatedByUserId,
                request.Name,
                request.Amount,
                request.DueDate,
                request.Recurrence,
                request.Category,
                request.NotifyDaysBefore);

            var billId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/households/{householdId}/bills/{billId}", new { id = billId });
        });

        group.MapPut("/{billId:guid}", async (Guid householdId, Guid billId, UpdateBillRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var command = new UpdateBillReminderCommand(
                householdId,
                billId,
                request.Name,
                request.Amount,
                request.DueDate,
                request.Recurrence,
                request.Category,
                request.NotifyDaysBefore);

            var updated = await sender.Send(command, cancellationToken);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{billId:guid}", async (Guid householdId, Guid billId, ISender sender, CancellationToken cancellationToken) =>
        {
            var deleted = await sender.Send(new DeleteBillCommand(householdId, billId), cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapPost("/bulk-delete", async (Guid householdId, BulkDeleteBillsRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var ids = request.Ids?.Distinct().ToList() ?? [];
            if (ids.Count == 0 || ids.Count > MaxBulkDeleteCount)
            {
                return Results.BadRequest($"Select between 1 and {MaxBulkDeleteCount} bills.");
            }

            var deletedCount = await sender.Send(new DeleteBillsCommand(householdId, ids), cancellationToken);
            return Results.Ok(new { deletedCount });
        });

        group.MapPost("/{billId:guid}/mark-paid", async (Guid householdId, Guid billId, ISender sender, CancellationToken cancellationToken) =>
        {
            var updated = await sender.Send(new MarkBillPaidCommand(householdId, billId), cancellationToken);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}

public sealed record CreateBillRequest(
    Guid CreatedByUserId,
    string Name,
    decimal Amount,
    DateOnly DueDate,
    BillRecurrence Recurrence,
    string Category,
    int NotifyDaysBefore);

public sealed record BulkDeleteBillsRequest(IReadOnlyList<Guid>? Ids);

public sealed record UpdateBillRequest(
    string Name,
    decimal Amount,
    DateOnly DueDate,
    BillRecurrence Recurrence,
    string Category,
    int NotifyDaysBefore);
