using Mediator;
using Resit.Modules.Bills.Application.Abstractions;

namespace Resit.Modules.Bills.Application.DeleteBills;

public sealed class DeleteBillsCommandHandler(IBillRepository repository) : IRequestHandler<DeleteBillsCommand, int>
{
    public async ValueTask<int> Handle(DeleteBillsCommand command, CancellationToken cancellationToken)
    {
        var bills = await repository.GetByIdsAsync(command.BillIds, command.HouseholdId, cancellationToken);
        if (bills.Count == 0)
        {
            return 0;
        }

        repository.RemoveRange(bills);
        await repository.SaveChangesAsync(cancellationToken);

        return bills.Count;
    }
}
