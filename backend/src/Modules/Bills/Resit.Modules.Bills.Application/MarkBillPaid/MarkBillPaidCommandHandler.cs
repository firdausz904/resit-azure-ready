using Mediator;
using Resit.Modules.Bills.Application.Abstractions;

namespace Resit.Modules.Bills.Application.MarkBillPaid;

public sealed class MarkBillPaidCommandHandler(IBillRepository repository) : IRequestHandler<MarkBillPaidCommand, bool>
{
    public async ValueTask<bool> Handle(MarkBillPaidCommand command, CancellationToken cancellationToken)
    {
        var bill = await repository.GetAsync(command.BillId, command.HouseholdId, cancellationToken);
        if (bill is null)
        {
            return false;
        }

        bill.MarkPaid();
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
