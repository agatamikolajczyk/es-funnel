using Funnel.EventsStore;

namespace Funnel.Core;

public class User : Aggregate
{
    public string Name { get; private set; } = default!;

    public User(Guid id, string name)
    {
        var @event = new UserCreated(id, name);

        Enqueue(@event);
        Apply(@event);
    }

    // For serialization
    private User()
    {
    }

    public void ChangeName(string name)
    {
        var @event = new UserNameUpdated(Id, name);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(UserCreated @event)
    {
        Id = @event.UserId;
        Name = @event.UserName;
    }

    public void Apply(UserNameUpdated @event)
    {
        Name = @event.UserName;
    }
}

public class UserCreated
{
    public Guid UserId { get; }
    public string UserName { get; }

    public UserCreated(Guid userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}

public class UserNameUpdated
{
    public Guid UserId { get; }
    public string UserName { get; }

    public UserNameUpdated(Guid userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}