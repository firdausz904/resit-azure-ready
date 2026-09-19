using Mediator;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Application.DeleteReceipts;

public sealed class DeleteReceiptsCommandHandler(IReceiptRepository repository, IReceiptFileStorage fileStorage)
    : IRequestHandler<DeleteReceiptsCommand, int>
{
    public async ValueTask<int> Handle(DeleteReceiptsCommand command, CancellationToken cancellationToken)
    {
        var receipts = await repository.GetByIdsAsync(command.ReceiptIds, command.HouseholdId, cancellationToken);
        if (receipts.Count == 0)
        {
            return 0;
        }

        var storagePaths = receipts.Select(r => r.StoragePath).ToList();

        repository.RemoveRange(receipts);
        await repository.SaveChangesAsync(cancellationToken);

        foreach (var storagePath in storagePaths)
        {
            await fileStorage.DeleteAsync(storagePath, cancellationToken);
        }

        return receipts.Count;
    }
}
