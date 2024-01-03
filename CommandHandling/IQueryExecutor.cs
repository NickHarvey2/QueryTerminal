using System.Data;

namespace QueryTerminal.CommandHandling;

public interface IQueryExecutor<TConnection> where TConnection : IDbConnection
{
    Task<IDataReader> Execute(TConnection conn, string sqlQuery, CancellationToken cancellationToken);
}
