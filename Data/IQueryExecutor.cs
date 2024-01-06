using System.Data;

namespace QueryTerminal.Data;

public interface IQueryExecutor<TConnection> where TConnection : IDbConnection
{
    Task<IDataReader> Execute(TConnection conn, string sqlQuery, CancellationToken cancellationToken);
}
