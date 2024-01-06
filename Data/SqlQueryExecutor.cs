using System.Data;
using Microsoft.Data.SqlClient;

namespace QueryTerminal.Data;

public class SqlQueryExecutor : IQueryExecutor<SqlConnection>
{
    public async Task<IDataReader> Execute(SqlConnection conn, string sqlQuery, CancellationToken cancellationToken)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sqlQuery;
        return await cmd.ExecuteReaderAsync(cancellationToken);
    }
}
