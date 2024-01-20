using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace QueryTerminal.Data;

public abstract class QueryTerminalDbConnection<TConnection> : IQueryTerminalDbConnection where TConnection : DbConnection, new()
{
    protected TConnection _connection;

    public QueryTerminalDbConnection(IConfiguration configuration)
    {
        _connection = new TConnection();
        _connection.ConnectionString = configuration["connectionString"];
    }

    public virtual async Task ConnectAsync(CancellationToken cancellationToken)
    {
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
