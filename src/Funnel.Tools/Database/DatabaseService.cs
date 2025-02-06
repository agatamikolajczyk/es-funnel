using System.Diagnostics;
using Dapper;
using Npgsql;

namespace Funnel.Tools.Database;

public class DatabaseService
{
    private readonly string _connectionString;
    public DatabaseService(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public static NpgsqlConnection GetFreshDbConnection(string connectionString)
    {
        // get the test class name that will be used as POSTGRES schema
        var schema = "Funnel";
        // each test will have it's own schema name to run have data isolation and not interfere other tests
        var connection = new NpgsqlConnection(connectionString + $"Search Path= '{schema}'");

        // recreate schema to have it fresh for tests. Kids do not try that on production.
        connection.Execute($"CREATE SCHEMA IF NOT EXISTS {schema};");

        return connection;
    }
}