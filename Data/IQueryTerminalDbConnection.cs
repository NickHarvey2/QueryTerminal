using System.Data.Common;
using System.Text.RegularExpressions;

namespace QueryTerminal.Data;

public interface IQueryTerminalDbConnection : IAsyncDisposable
{
    public Task OpenAsync(CancellationToken cancellationToken);
    public Task<DbDataReader> ExecuteQueryAsync(string commandText, CancellationToken cancellationToken);
    public Task<IEnumerable<DbColumn>> GetColumnsAsync(string tableName, CancellationToken cancellationToken);
    public Task<IEnumerable<DbTable>> GetTablesAsync(CancellationToken cancellationToken);
    public Regex KeywordsRx { get; }
}
