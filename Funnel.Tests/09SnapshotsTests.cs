/*using System.Linq.Expressions;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Funnel.Core;
using Funnel.Core.Carts;
using Funnel.EventsStore;
using Funnel.Tools.Tools;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Xunit;
namespace Funnel.Tests;

public class SnapshotsTests
{
        private readonly NpgsqlConnection databaseConnection;
        private readonly EventStore eventStore;
        private readonly IRepository<Funnel.Core.Carts.Cart> repository;
    
        /// <summary>
        /// Inits Event Store
        /// </summary>
        public SnapshotsTests()
        {
            databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

            var databaseProvider =
                new PostgresqlDatabaseProvider(databaseConnection) { SchemaName = typeof(Exercise08Snapshots).Name };

            var migrationsAssembly = typeof(SnapshotsTests).Assembly;
            var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
            migrator.Load();
            migrator.MigrateToLatest();

            // Create Event Store
            eventStore = new EventStore(databaseConnection);

            var userSnapshot = new SnapshotToTable<User>(
                databaseConnection,
                @"TODO write upsert here");

            eventStore.AddSnapshot(userSnapshot);

            // Initialize Event Store
            eventStore.Init();

            repository = new Repository<User>(eventStore);
        }
    
        [Fact]
        [Trait("Category", "SkipCI")]
        public void AddingAndUpdatingAggregate_ShouldCreateAndUpdateSnapshotAccordingly()
        {
            var streamId = Guid.NewGuid();
            var cart = new Cart(streamId, "John Doe");
    
            repository.Add(cart);
    
            var cartFromDB = databaseConnection.Get<Cart>(streamId);
    
            cartFromDB.Should().NotBeNull();
            cartFromDB.Id.Should().Be(streamId);
            cartFromDB.Username.Should().Be("John Doe");
            cartFromDB.Version.Should().Be(1);
    
            cartFromDB.AddNewItem("abc",4);
    
            repository.Update(cartFromDB);
    
            var userAfterUpdate = databaseConnection.Get<Cart>(streamId);
    
            userAfterUpdate.Id.Should().Be(streamId);
            userAfterUpdate.Items.Count.Should().Be(1);
            cartFromDB.Version.Should().Be(2);
        }
    
        [Fact]
        [Trait("Category", "SkipCI")]
        public void Snapshots_ShouldBeQueryable()
        {
            const string john = "John";
    
            var firstMatchingUser = new Cart(Guid.NewGuid(), $"{john} Doe");
            var secondMatchingUser = new Cart(Guid.NewGuid(), $"{john} Smith");
            var notMatchingUser = new Cart(Guid.NewGuid(), "Anna Smith");
    
            repository.Add(firstMatchingUser);
            repository.Add(secondMatchingUser);
            repository.Add(notMatchingUser);
    
            var users = databaseConnection.Query<Cart>(
                @"SELECT id, name, version
                        FROM CARTS
                        WHERE username LIKE '%" + john + "%'");
    
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
}*/