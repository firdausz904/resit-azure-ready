using Mediator;
using Resit.Modules.Receipts.Application.Abstractions;

namespace Resit.Modules.Receipts.Application.EditReceipt;

public sealed class EditReceiptCommandHandler(IReceiptRepository repository) : IRequestHandler<EditReceiptCommand, bool>
{
    public async ValueTask<bool> Handle(EditReceiptCommand command, CancellationToken cancellationToken)
    {
        var receipt = await repository.GetAsync(command.ReceiptId, command.HouseholdId, cancellationToken);
        if (receipt is null)
        {
            return false;
        }

        receipt.Edit(command.Merchant, command.Total, command.PurchasedOn, command.Category);
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
