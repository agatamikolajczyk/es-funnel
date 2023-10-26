namespace Funnel.EventsStore;

public interface ISnapshot
{
    Type Handles { get; }
    void Handle(IAggregate aggregate);
}