using System.Data;
using Microsoft.Data.Sqlite;

namespace QueryTerminal.Data;

public class SqliteQueryExecutor : IQueryExecutor<SqliteConnection>
{
    public async Task<IDataReader> Execute(SqliteConnection conn, string sqlQuery, CancellationToken cancellationToken)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sqlQuery;
        return await cmd.ExecuteReaderAsync(cancellationToken);
    }
}

