using Mediator;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Application.DeleteReceipt;

public sealed class DeleteReceiptCommandHandler(IReceiptRepository repository, IReceiptFileStorage fileStorage)
    : IRequestHandler<DeleteReceiptCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteReceiptCommand command, CancellationToken cancellationToken)
    {
        var receipt = await repository.GetAsync(command.ReceiptId, command.HouseholdId, cancellationToken);
        if (receipt is null)
        {
            return false;
        }

        var storagePath = receipt.StoragePath;

        repository.Remove(receipt);
        await repository.SaveChangesAsync(cancellationToken);

        await fileStorage.DeleteAsync(storagePath, cancellationToken);

        return true;
    }
}
