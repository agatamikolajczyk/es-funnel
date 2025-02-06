using FluentAssertions;
using Funnel.Core.Carts;
using Funnel.Core.Carts.Events;
using Funnel.Tests.Tools;
using Funnel.Tools.Tools;
using Npgsql;
using Xunit;

namespace Funnel.Tests;

public class EventStoreMethodsTests
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly EventsStore.EventStore eventStore;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public EventStoreMethodsTests()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection(Settings.ConnectionString);

        // Create Event Store
        eventStore = new EventsStore.EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void GetStreamState_ShouldReturnProperStreamInfo()
    {
        var streamId = Guid.NewGuid();
        var @event = new CartCreated(streamId,"krzysztof.jarzyna");

        eventStore.AppendEvent<CartCreated>(streamId, @event);

        var streamState = eventStore.GetStreamState(streamId);

        streamState.Id.Should().Be(streamId);
        streamState.Type.Should().Be(typeof(CartCreated));
        streamState.Version.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void GetEvents_ShouldReturnAppendedEvents()
    {
        var streamId = Guid.NewGuid();
        var cartCreated = new CartCreated(streamId,"krzysztof.jarzyna");
        var itemAdded = new ItemAdded(streamId, "abc", 5);

        eventStore.AppendEvent<Cart>(streamId, cartCreated);
        eventStore.AppendEvent<Cart>(streamId, itemAdded);

        var events = eventStore.GetEvents(streamId);

        events.Cast<object>().Should().HaveCount(2);

        events.OfType<CartCreated>().Should().Contain(
            e => e.CartId == cartCreated.CartId);

        events.OfType<ItemAdded>().Should().Contain(
            e => e.CartId == itemAdded.CartId && e.Index == itemAdded.Index && e.Quantity == itemAdded.Quantity);
    }
}