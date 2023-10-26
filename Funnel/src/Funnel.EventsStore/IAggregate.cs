namespace Funnel.EventsStore;

public interface IAggregate
{
    Guid Id { get; }
    int Version { get; }
    IEnumerable<object> DequeueUncommittedEvents();
}