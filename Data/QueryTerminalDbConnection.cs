using System.Data.Common;

namespace QueryTerminal.Data;

public abstract class QueryTerminalDbConnection<TConnection> : IAsyncDisposable where TConnection : DbConnection, new()
{
    protected TConnection _connection;

    public QueryTerminalDbConnection()
    {
        _connection = new TConnection();
    }

    public virtual async Task ConnectAsync(string connectionString, CancellationToken cancellationToken)
    {
        _connection.ConnectionString = connectionString;
        await _connection.OpenAsync(cancellationToken);
    }

    public async Task<DbDataReader> ExecuteQueryAsync(string commandText, CancellationToken cancellationToken)
    {
        var command = _connection.CreateCommand();
        command.CommandText = commandText;
        return await command.ExecuteReaderAsync(cancellationToken);
    }

    public abstract Task<IEnumerable<DbColumn>> GetColumnsAsync(string tableName, CancellationToken cancellationToken);
    public abstract Task<IEnumerable<DbTable>> GetTablesAsync(CancellationToken cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
        {
            return;
        }
        await _connection.DisposeAsync();
    }
}
