namespace Funnel.EventsStore;

public class StreamState
{
    public Guid Id { get; }

    public Type Type { get; } = null!;

    public long Version { get; }

    public StreamState()
    {
    }

    public StreamState(Guid id, Type type, long version)
    {
        Id = id;
        Type = type;
        Version = version;
    }
}