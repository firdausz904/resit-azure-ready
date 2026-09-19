namespace Resit.SharedKernel;

public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();

    private readonly List<object> _domainEvents = [];

    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(object domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
