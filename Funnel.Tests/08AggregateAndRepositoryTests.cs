using FluentAssertions;
using Funnel.Core;
using Funnel.Core.Carts;
using Funnel.EventsStore;
using Funnel.Tests.Tools;
using Funnel.Tools.Tools;
using Npgsql;
using Xunit;

namespace Funnel.Tests;

public class AggregateAndRepositoryTests
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;
    private readonly IRepository<Cart> repository;

    public AggregateAndRepositoryTests()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection(Settings.ConnectionString);

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();

        repository = new Repository<Cart>(eventStore);
    }
    
    [Fact]
    [Trait("Category", "SkipCI")]
    public void Repository_FullFlow_ShouldSucceed()
    {
        var streamId = Guid.NewGuid();
        var cart = new Cart(streamId, "krzysztof.jarzyna");

        repository.Add(cart);

        var cartFromRepository = repository.Find(streamId);

        cartFromRepository.Id.Should().Be(streamId);
        cartFromRepository.Username.Should().Be("krzysztof.jarzyna");
        cartFromRepository.Version.Should().Be(1);

        cartFromRepository.SetPaymentMethod("BLIK");

        repository.Update(cartFromRepository);

        var userAfterUpdate = repository.Find(streamId);

        userAfterUpdate.Id.Should().Be(streamId);
        userAfterUpdate.PaymentMethod.Should().Be("BLIK");
        cartFromRepository.Version.Should().Be(2);
    }
}