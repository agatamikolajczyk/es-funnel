using Dapper;
using FluentAssertions;
using Funnel.Core.Carts;
using Funnel.Core.Carts.Events;
using Funnel.Tools.Tools;
using Npgsql;
using Xunit;

namespace Funnel.Tests;

public class CreateAppendEventFunctionTests
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly PostgresSchemaProvider schemaProvider;
    private readonly EventsStore.EventStore eventStore;

    private const string AppendEventFunctionName = "append_event";

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public CreateAppendEventFunctionTests()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
        schemaProvider = new PostgresSchemaProvider(databaseConnection);

        // Create Event Store
        eventStore = new EventsStore.EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void AppendEventFunction_ShouldBeCreated()
    {
        var appendFunctionExists = schemaProvider
            .FunctionExists(AppendEventFunctionName);

        appendFunctionExists.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void AppendEventFunction_WhenStreamDoesNotExist_CreateNewStream_And_AppendNewEvent()
    {
        var streamId = Guid.NewGuid();
        var @event = new CartCreated(streamId, "krzysztof.jarzyna");

        var result = eventStore.AppendEvent<Cart>(streamId, @event);

        result.Should().BeTrue();

        var wasStreamCreated = databaseConnection.QuerySingle<bool>(
            "select exists (select 1 from streams where id = @streamId)", new {streamId}
        );
        wasStreamCreated.Should().BeTrue();

        var wasEventAppended = databaseConnection.QuerySingle<bool>(
            "select exists (select 1 from events where stream_id = @streamId)", new {streamId}
        );
        wasEventAppended.Should().BeTrue();
    }
}