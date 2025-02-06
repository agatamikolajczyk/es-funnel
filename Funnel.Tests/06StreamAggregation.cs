using FluentAssertions;
using Funnel.Core.Carts;
using Funnel.Core.Carts.Events;
using Funnel.Tests.Tools;
using Funnel.Tools.Tools;
using Npgsql;
using Xunit;

namespace Funnel.Tests;

public class StreamAggregation
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly EventsStore.EventStore eventStore;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public StreamAggregation()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection(Settings.ConnectionString);

        // Create Event Store
        eventStore = new EventsStore.EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void AggregateStream_ShouldReturnObjectWithStateBasedOnEvents()
    {
        var streamId = Guid.NewGuid();
        var cartCreated = new CartCreated(streamId,"krzysztof.jarzyna");
        var firstItemAdded = new ItemAdded (streamId,"abc", 5);
        var secondItemAdded = new ItemAdded (streamId,"def", 3);
        

        eventStore.AppendEvent<Cart>(streamId, cartCreated);
        eventStore.AppendEvent<Cart>(streamId, firstItemAdded);
        eventStore.AppendEvent<Cart>(streamId, secondItemAdded);

        var aggregate = eventStore.AggregateStream<Cart>(streamId);

        aggregate.Id.Should().Be(streamId);
        aggregate.Items.Count.Should().Be(2);
        aggregate.Items.First().Index.Should().Be(firstItemAdded.Index);
        aggregate.Items.First().Quantity.Should().Be(firstItemAdded.Quantity);
        aggregate.Version.Should().Be(3);
    }
}