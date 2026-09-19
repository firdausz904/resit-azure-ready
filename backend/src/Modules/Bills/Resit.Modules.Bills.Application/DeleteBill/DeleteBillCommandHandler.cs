using Mediator;
using Resit.Modules.Bills.Application.Abstractions;

namespace Resit.Modules.Bills.Application.DeleteBill;

public sealed class DeleteBillCommandHandler(IBillRepository repository) : IRequestHandler<DeleteBillCommand, bool>
{
    public async ValueTask<bool> Handle(DeleteBillCommand command, CancellationToken cancellationToken)
    {
        var bill = await repository.GetAsync(command.BillId, command.HouseholdId, cancellationToken);
        if (bill is null)
        {
            return false;
        }

        repository.Remove(bill);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
