using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Resit.Modules.Receipts.Application.DeleteReceipt;
using Resit.Modules.Receipts.Application.DeleteReceipts;
using Resit.Modules.Receipts.Application.EditReceipt;
using Resit.Modules.Receipts.Application.GetDashboardSummary;
using Resit.Modules.Receipts.Application.GetReceiptById;
using Resit.Modules.Receipts.Application.GetReceipts;
using Resit.Modules.Receipts.Application.UploadReceipts;
using Resit.Modules.Identity;

namespace Resit.Modules.Receipts.Api;

public static class ReceiptsEndpoints
{
    private const long MaxFilesPerBatch = 100;
    private const int MaxBulkDeleteCount = 100;

    public static IEndpointRouteBuilder MapReceiptsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/households/{householdId:guid}/receipts").WithTags("Receipts").RequireHouseholdMatch();

        group.MapPost("/batches", async (
            Guid householdId,
            Guid uploadedByUserId,
            IFormFileCollection files,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (files.Count == 0 || files.Count > MaxFilesPerBatch)
            {
                return Results.BadRequest($"Upload between 1 and {MaxFilesPerBatch} files.");
            }

            var uploadedFiles = files
                .Select(f => new UploadedFile(f.FileName, f.OpenReadStream()))
                .ToList();

            var command = new UploadReceiptsCommand(householdId, uploadedByUserId, uploadedFiles);
            var result = await sender.Send(command, cancellationToken);

            return Results.Accepted($"/api/households/{householdId}/batches/{result.BatchId}", result);
        }).DisableAntiforgery();

        group.MapGet("/", async (
            Guid householdId,
            string? search,
            string? category,
            DateOnly? from,
            DateOnly? to,
            int page,
            int pageSize,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetReceiptsQuery(
                householdId,
                search,
                category,
                from,
                to,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize);

            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        });

        group.MapGet("/{receiptId:guid}", async (
            Guid householdId,
            Guid receiptId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var receipt = await sender.Send(new GetReceiptByIdQuery(householdId, receiptId), cancellationToken);
            return receipt is null ? Results.NotFound() : Results.Ok(receipt);
        });

        group.MapPut("/{receiptId:guid}", async (
            Guid householdId,
            Guid receiptId,
            EditReceiptRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new EditReceiptCommand(householdId, receiptId, request.Merchant, request.Total, request.PurchasedOn, request.Category);
            var updated = await sender.Send(command, cancellationToken);
            return updated ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{receiptId:guid}", async (
            Guid householdId,
            Guid receiptId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var deleted = await sender.Send(new DeleteReceiptCommand(householdId, receiptId), cancellationToken);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapPost("/bulk-delete", async (
            Guid householdId,
            BulkDeleteReceiptsRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var ids = request.Ids?.Distinct().ToList() ?? [];
            if (ids.Count == 0 || ids.Count > MaxBulkDeleteCount)
            {
                return Results.BadRequest($"Select between 1 and {MaxBulkDeleteCount} receipts.");
            }

            var deletedCount = await sender.Send(new DeleteReceiptsCommand(householdId, ids), cancellationToken);
            return Results.Ok(new { deletedCount });
        });

        var dashboardGroup = app.MapGroup("/api/households/{householdId:guid}").WithTags("Dashboard").RequireHouseholdMatch();

        dashboardGroup.MapGet("/dashboard-summary", async (
            Guid householdId,
            DateOnly from,
            DateOnly to,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var summary = await sender.Send(new GetDashboardSummaryQuery(householdId, from, to), cancellationToken);
            return Results.Ok(summary);
        });

        return app;
    }
}

public sealed record BulkDeleteReceiptsRequest(IReadOnlyList<Guid>? Ids);

public sealed record EditReceiptRequest(string Merchant, decimal Total, DateOnly PurchasedOn, string Category);
