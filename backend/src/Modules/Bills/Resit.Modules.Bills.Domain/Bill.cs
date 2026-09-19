using Resit.SharedKernel;

namespace Resit.Modules.Bills.Domain;

public sealed class Bill : Entity
{
    public Guid HouseholdId { get; private init; }
    public Guid CreatedByUserId { get; private init; }
    public string Name { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public BillRecurrence Recurrence { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public int NotifyDaysBefore { get; private set; }
    public bool IsPaid { get; private set; }
    public DateTimeOffset? LastNotifiedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }

    private Bill() { }

    public static Bill Create(
        Guid householdId,
        Guid createdByUserId,
        string name,
        decimal amount,
        DateOnly dueDate,
        BillRecurrence recurrence,
        string category,
        int notifyDaysBefore) => new()
    {
        Id = Guid.NewGuid(),
        HouseholdId = householdId,
        CreatedByUserId = createdByUserId,
        Name = name,
        Amount = amount,
        DueDate = dueDate,
        Recurrence = recurrence,
        Category = category,
        NotifyDaysBefore = notifyDaysBefore,
        IsPaid = false,
        CreatedAt = DateTimeOffset.UtcNow
    };

    public void Update(string name, decimal amount, DateOnly dueDate, BillRecurrence recurrence, string category, int notifyDaysBefore)
    {
        Name = name;
        Amount = amount;
        DueDate = dueDate;
        Recurrence = recurrence;
        Category = category;
        NotifyDaysBefore = notifyDaysBefore;
    }

    public void MarkPaid()
    {
        IsPaid = true;

        if (Recurrence != BillRecurrence.OneTime)
        {
            DueDate = NextOccurrence();
            IsPaid = false;
            LastNotifiedAt = null;
        }
    }

    public bool ShouldNotify(DateOnly today) =>
        !IsPaid &&
        today >= DueDate.AddDays(-NotifyDaysBefore) &&
        LastNotifiedAt?.Date != DateTimeOffset.UtcNow.Date;

    public void MarkNotified() => LastNotifiedAt = DateTimeOffset.UtcNow;

    private DateOnly NextOccurrence() => Recurrence switch
    {
        BillRecurrence.Weekly => DueDate.AddDays(7),
        BillRecurrence.Monthly => DueDate.AddMonths(1),
        BillRecurrence.Quarterly => DueDate.AddMonths(3),
        BillRecurrence.Yearly => DueDate.AddYears(1),
        _ => DueDate
    };
}
