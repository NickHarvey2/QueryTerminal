using System.Data;
using Microsoft.Data.SqlClient;

namespace QueryTerminal.Data;

public class SqlQueryExecutor : IQueryExecutor<SqlConnection>
{
    private readonly IDbConnectionProvider<SqlConnection> _connectionProvider;
    private SqlConnection _connection;

    public SqlQueryExecutor(IDbConnectionProvider<SqlConnection> dbConnectionProvider)
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
