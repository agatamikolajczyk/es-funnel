using FluentAssertions;
using Funnel.Core.Carts;
using Funnel.Core.Carts.Events;
using Funnel.Tools.Tools;
using Npgsql;
using Xunit;

namespace Funnel.Tests;

public class TimeTravelling
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly EventsStore.EventStore eventStore;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public TimeTravelling()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventsStore.EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void AggregateStream_ShouldReturnSpecifiedVersionOfTheStream()
    {
        var streamId = Guid.NewGuid();
        var cartCreated = new CartCreated(streamId,"krzysztof.jarzyna");
        var itemAdded = new ItemAdded(streamId,  "abc",  3);
        var itemAddedAgain = new ItemAdded(streamId, "def", 5);

        eventStore.AppendEvent<Cart>(streamId, cartCreated);
        eventStore.AppendEvent<Cart>(streamId, itemAdded);
        eventStore.AppendEvent<Cart>(streamId, itemAddedAgain);

        var aggregateAtVersion1 = eventStore.AggregateStream<Cart>(streamId, 1);

        aggregateAtVersion1.Id.Should().Be(streamId);
        aggregateAtVersion1.Version.Should().Be(1);

        var aggregateAtVersion2 = eventStore.AggregateStream<Cart>(streamId, 2);

        aggregateAtVersion2.Id.Should().Be(streamId);
        aggregateAtVersion2.Version.Should().Be(2);

        var aggregateAtVersion3 = eventStore.AggregateStream<Cart>(streamId, 3);

        aggregateAtVersion3.Id.Should().Be(streamId);
        aggregateAtVersion3.Version.Should().Be(3);
    }
}