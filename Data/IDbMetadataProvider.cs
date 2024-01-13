using System.Data.Common;

namespace QueryTerminal.Data;

public interface IDbMetadataProvider<TConnection> where TConnection : DbConnection
{
    // part of the purpose of this is to support completion in the REPL
    // since the schema doesn't change frequently and we want the completion
    // menu to load as quickly as possible, consider memoizing the result
    Task<IEnumerable<DbTable>> GetTables(TConnection connection, CancellationToken cancellationToken);

    // For similar reasons, consider memoization in the implementation of this method
    Task<IEnumerable<DbColumn>> GetColumns(TConnection connection, string tableName, CancellationToken cancellationToken);

    // TODO: additional methods to support more detailed metadata, e.g. list all tables with column names and types
    // TODO: figure out a reasonable way to use the currently selected output format
}
