using FluentAssertions;
using Funnel.Core;
using Funnel.EventsStore;
using Funnel.Tools.Tools;
using Npgsql;
using Xunit;

namespace Funnel.Tests;

public class AggregateAndRepositoryTests
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;
    private readonly IRepository<User> repository;

    public AggregateAndRepositoryTests()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();

        repository = new Repository<User>(eventStore);
    }
    
    [Fact]
    [Trait("Category", "SkipCI")]
    public void Repository_FullFlow_ShouldSucceed()
    {
        var streamId = Guid.NewGuid();
        var user = new User(streamId, "John Doe");

        repository.Add(user);

        var userFromRepository = repository.Find(streamId);

        userFromRepository.Id.Should().Be(streamId);
        userFromRepository.Name.Should().Be("John Doe");
        userFromRepository.Version.Should().Be(1);

        userFromRepository.ChangeName("Adam Smith");

        repository.Update(userFromRepository);

        var userAfterUpdate = repository.Find(streamId);

        userAfterUpdate.Id.Should().Be(streamId);
        userAfterUpdate.Name.Should().Be("Adam Smith");
        userFromRepository.Version.Should().Be(2);
    }
}