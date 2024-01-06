using System.Data;
using Microsoft.Data.Sqlite;

namespace QueryTerminal.Data;

public class SqliteQueryExecutor : IQueryExecutor<SqliteConnection>
{
    private readonly IDbConnectionProvider<SqliteConnection> _connectionProvider;
    private SqliteConnection _connection;

    public SqliteQueryExecutor(IDbConnectionProvider<SqliteConnection> dbConnectionProvider)
    {
        _connectionProvider = dbConnectionProvider;
    }

    public void Connect(string connectionString)
    {
        _connection = _connectionProvider.Connect(connectionString);
        _connection.Open();
    }

    public async Task<IDataReader> Execute(string sqlQuery, CancellationToken cancellationToken)
    {
        var cmd = _connection.CreateCommand();
        cmd.CommandText = sqlQuery;
        return await cmd.ExecuteReaderAsync(cancellationToken);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
