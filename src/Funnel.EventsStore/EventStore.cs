using System.Collections;
using System.Data;
using Dapper;
using Funnel.Tools.Tools;
using Newtonsoft.Json;
using Npgsql;

namespace Funnel.EventsStore;

public class EventStore : IDisposable, IEventStore
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly IList<ISnapshot> snapshots = new List<ISnapshot>();
    private const string Apply = "Apply";
    public EventStore(NpgsqlConnection databaseConnection)
    {
        this.databaseConnection = databaseConnection;
    }

    public void Init()
    {
        CreateStreamsTable();
        CreateEventsTable();
        CreateAppendEventFunction();
    }
    
    public void AddSnapshot(ISnapshot snapshot)
    {
        snapshots.Add(snapshot);
    }
    
    public bool Store<TStream>(TStream aggregate) where TStream : IAggregate
    {
        var events = aggregate.DequeueUncommittedEvents();
        var initialVersion = aggregate.Version - events.Count();

        foreach (var @event in events)
        {
            AppendEvent<TStream>(aggregate.Id, @event, initialVersion++);
        }

        snapshots
            .FirstOrDefault(snapshot => snapshot.Handles == typeof(TStream))?
            .Handle(aggregate);

        return true;
    }

    public bool AppendEvent<TStream>(Guid streamId, object @event, long? expectedVersion = null)
    {
        return databaseConnection.QuerySingle<bool>(
            "SELECT append_event(@Id, @Data::jsonb, @Type, @StreamId, @StreamType, @ExpectedVersion)",
            new
            {
                Id = Guid.NewGuid(),
                Data = JsonConvert.SerializeObject(@event),
                Type = @event.GetType().AssemblyQualifiedName,
                StreamId = streamId,
                StreamType = typeof(TStream).AssemblyQualifiedName,
                ExpectedVersion = expectedVersion
            },
            commandType: CommandType.Text
        );
    }
    
    public T AggregateStream<T>(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp = null) where T: notnull
    {
        var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

        var events = GetEvents(streamId, atStreamVersion, atTimestamp);
        var version = 0;

        foreach (var @event in events)
        {
            aggregate.InvokeIfExists(Apply, @event);
            aggregate.SetIfExists(nameof(IAggregate.Version), ++version);
        }

        return aggregate;
    }

    public StreamState GetStreamState(Guid streamId)
    {
        const string getStreamSql =
            @"SELECT id, type, version
                  FROM streams
                  WHERE id = @streamId";

        return databaseConnection
            .Query<dynamic>(getStreamSql, new { streamId })
            .Select(streamData =>
                new StreamState(
                    streamData.id,
                    Type.GetType(streamData.type),
                    streamData.version
                ))
            .SingleOrDefault();
    }

    public IEnumerable GetEvents(Guid streamId, long? atStreamVersion = null, DateTime? atTimestamp=null )
    {
        const string getStreamSql =
            @"SELECT id, data, stream_id, type, version, created
                  FROM events
                  WHERE stream_id = @streamId
                  AND (@atStreamVersion IS NULL OR version <= @atStreamVersion)
                  AND (@atTimestamp IS NULL OR created <= @atTimestamp)
                  ORDER BY version";

        atTimestamp = atTimestamp ?? DateTime.Now;

            return databaseConnection
            .Query<dynamic>(getStreamSql, new { streamId, atStreamVersion, atTimestamp })
            .Select(@event =>
                JsonConvert.DeserializeObject(
                    @event.data,
                    Type.GetType(@event.type)
                ))
            .ToList();
    }

    
    
    private void CreateStreamsTable()
    {
        const string creatStreamsTableSql =
            @"CREATE TABLE IF NOT EXISTS streams(
                      id             UUID                      NOT NULL    PRIMARY KEY,
                      type           TEXT                      NOT NULL,
                      version        BIGINT                    NOT NULL
                  );";
        databaseConnection.Execute(creatStreamsTableSql);
    }

    private void CreateEventsTable()
    {
        const string creatStreamsTableSql =
            @"CREATE TABLE IF NOT EXISTS events(
                      id             UUID                      NOT NULL    PRIMARY KEY,
                      data           JSONB                     NOT NULL,
                      stream_id      UUID                      NOT NULL,
                      type           TEXT                      NOT NULL,
                      version        BIGINT                    NOT NULL,
                      created        timestamp with time zone  NOT NULL    default (now()),
                      FOREIGN KEY(stream_id) REFERENCES streams(id),
                      CONSTRAINT events_stream_and_version UNIQUE(stream_id, version)
                );";
        databaseConnection.Execute(creatStreamsTableSql);
    }

    private void CreateAppendEventFunction()
    {
        const string appendEventFunctionSql =
            @"CREATE OR REPLACE FUNCTION append_event(
                    event_id uuid,
                    data jsonb,
                    type text,
                    stream_id uuid,
                    stream_type text,
                    expected_stream_version bigint default null
                ) RETURNS boolean
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    stream_version int;
                BEGIN
                    INSERT INTO streams (id, type, version)
                    VALUES (stream_id, stream_type, 0) 
                    ON CONFLICT (id) do nothing;
                   
                    SELECT version INTO stream_version
                    FROM streams 
                    WHERE id = stream_id;

                    IF expected_stream_version IS NOT NULL AND stream_version != expected_stream_version THEN
                        RETURN FALSE;
                    END IF;

                    stream_version := stream_version + 1;

                    INSERT INTO events (id, data, stream_id, type, version, created)
                    VALUES (event_id, data, stream_id, type, stream_version, NOW());

                    UPDATE streams SET version = stream_version;

                   RETURN TRUE;
                END;
                $$;";
        databaseConnection.Execute(appendEventFunctionSql);
    }


    public void Dispose()
    {
        databaseConnection.Dispose();
    }
}