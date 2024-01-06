using System.Data;

namespace QueryTerminal.Data;

public interface IQueryExecutor<TConnection> : IDisposable where TConnection : IDbConnection
{
    void Connect(string connectionString);
    Task<IDataReader> Execute(string sqlQuery, CancellationToken cancellationToken);
}
