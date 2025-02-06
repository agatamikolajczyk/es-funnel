using System.Linq.Expressions;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Funnel.Core.Carts;
using Funnel.EventsStore;
using Funnel.Tests.Tools;
using Funnel.Tools.Tools;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Xunit;

namespace Funnel.Tests;

[Migration(1)]
public class CreateCarts : Migration
{
    protected override void Up()
    {
        Execute(@"CREATE TABLE IF NOT EXISTS Carts (
                Id UUID PRIMARY KEY,
                Username VARCHAR(100) NOT NULL,
                PaymentMethod VARCHAR(100),
                Version BIGINT NOT NULL);");
    }

    protected override void Down()
    {
        Execute("DROP TABLE IF EXISTS Carts;");
    }
}

public class SnapshotsTests
{
    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;
    private readonly IRepository<Cart> repository;


    /// <summary>
    /// Inits Event Store
    /// </summary>
    public SnapshotsTests()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection(Settings.ConnectionString);

        var databaseProvider =
            new PostgresqlDatabaseProvider(databaseConnection)
                { SchemaName = typeof(SnapshotsTests).Name ?? "SnapshotsTests" };

        // due to problems with migration temporary added in another way
        var migrationsAssembly = typeof(SnapshotsTests).Assembly;
        var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
        migrator.Load();
        migrator.MigrateToLatest();

        // databaseConnection.Execute(@"CREATE TABLE IF NOT EXISTS Carts (
        //         Id UUID PRIMARY KEY,
        //         Username VARCHAR(100) NOT NULL,
        //         PaymentMethod VARCHAR(100),
        //         Version BIGINT NOT NULL);");

        eventStore = new EventStore(databaseConnection);

        var cartSnapshot = new SnapshotToTable<Cart>(
            databaseConnection,
            @"INSERT INTO Carts (Id, Username, PaymentMethod, Version)
                        VALUES (@Id, @Username, @PaymentMethod, @Version)
                        ON CONFLICT (Id)
                        DO UPDATE SET
                        PaymentMethod = @PaymentMethod, Version = @Version");


        eventStore.AddSnapshot(cartSnapshot);

        // Initialize Event Store
        eventStore.Init();

        repository = new Repository<Cart>(eventStore);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void AddingAndUpdatingAggregate_ShouldCreateAndUpdateSnapshotAccordingly()
    {
        var streamId = Guid.NewGuid();
        var cart = new Cart(streamId, "krzysztof.jarzyna");

        repository.Add(cart);

        var cartFromDB = databaseConnection.Get<Cart>(streamId);

        cartFromDB.Should().NotBeNull();
        cartFromDB.Id.Should().Be(streamId);
        cartFromDB.Username.Should().Be("krzysztof.jarzyna");
        cartFromDB.Version.Should().Be(1);

        //todo checking how to add snapshot for aggregation
        //cartFromDB.AddNewItem("abc", 4);
        var paymentMethod = "Blik2";
        cartFromDB.SetPaymentMethod(paymentMethod);
        repository.Update(cartFromDB);

        var cartAfterUpdate = databaseConnection.Get<Cart>(streamId);

        cartAfterUpdate.Id.Should().Be(streamId);

        cartAfterUpdate.PaymentMethod.Should().Be(paymentMethod);
        cartFromDB.Version.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void Snapshots_ShouldBeQueryable()
    {
        const string username = "krzysztof";

        var firstMatchingUser = new Cart(Guid.NewGuid(), $"{username}.jarzyna");
        var secondMatchingUser = new Cart(Guid.NewGuid(), $"{username}.nowak");
        var notMatchingUser = new Cart(Guid.NewGuid(), "anna.kowalska");

        repository.Add(firstMatchingUser);
        repository.Add(secondMatchingUser);
        repository.Add(notMatchingUser);

        var users = databaseConnection.Query<Cart>(
            @"SELECT id, username, version
                        FROM CARTS
                        WHERE username LIKE '%" + username + "%'");

        users.Count().Should().Be(2);

        Expression<Func<Cart, bool>> UserEqualTo(Cart userToCompare)
        {
            return x => x.Id == userToCompare.Id
                        && x.Username == userToCompare.Username
                        && x.Version == userToCompare.Version;
        }

        users.Should().Contain(UserEqualTo(firstMatchingUser));
        users.Should().Contain(UserEqualTo(secondMatchingUser));
        users.Should().NotContain(UserEqualTo(notMatchingUser));
    }
}