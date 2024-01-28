using System.Data.Common;
using System.Text.RegularExpressions;

namespace QueryTerminal.Data;

public interface IQueryTerminalDbConnection : IAsyncDisposable
{
    public Task OpenAsync(CancellationToken cancellationToken);
    public Task<DbDataReader> ExecuteQueryAsync(string commandText, CancellationToken cancellationToken);
    public IEnumerable<DbColumn> GetColumns(string tableName);
    public IEnumerable<DbTable> GetTables();
    public Regex KeywordsRx { get; }
    public Regex FunctionsRx { get; }
}
