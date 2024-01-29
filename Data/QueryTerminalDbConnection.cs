using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace QueryTerminal.Data;

public abstract class QueryTerminalDbConnection<TConnection> : IQueryTerminalDbConnection where TConnection : DbConnection, new()
{
    private IEnumerable<DbTable>? _tables;
    private IDictionary<string,IEnumerable<DbColumn>>? _columns;
    protected TConnection _connection;
    private readonly Regex _keywordsRx;
    private readonly Regex _functionsRx;
    private Regex? _tablesRx;
    private Regex? _columnsRx;

    public QueryTerminalDbConnection(IConfiguration configuration)
    {
        _connection = new TConnection();
        _connection.ConnectionString = configuration["connectionString"];
        _keywordsRx = new Regex(
            $@"({string.Join(")|(", KeywordPatterns)})", 
            RegexOptions.IgnoreCase 
            | RegexOptions.CultureInvariant 
            | RegexOptions.ExplicitCapture 
            | RegexOptions.Compiled
        );
        _functionsRx = new Regex(
            $@"({string.Join(")|(", FunctionPatterns)})",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.ExplicitCapture
            | RegexOptions.Compiled
        );
    }

    public virtual async Task OpenAsync(CancellationToken cancellationToken)
    {
        await _connection.OpenAsync(cancellationToken);
        await RefreshAsync(cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await CacheTablesAsync(cancellationToken);
        await CacheColumnsAsync(cancellationToken);
        _tablesRx = new Regex(
            $@"({string.Join(")|(", _tables.Select(table => $@"\W(?<table>{table.Name})(\W|$)"))})",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.ExplicitCapture
            | RegexOptions.Compiled
        );
        _columnsRx = new Regex(
            $@"({string.Join(")|(", _columns.Values.SelectMany(colSet => colSet.Select(col => $@"\W(?<column>{col.Name})(\W|$)")))})",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.ExplicitCapture
            | RegexOptions.Compiled
        );
    }

    public async Task<DbDataReader> ExecuteQueryAsync(string commandText, CancellationToken cancellationToken)
    {
        var command = _connection.CreateCommand();
        command.CommandText = commandText;
        return await command.ExecuteReaderAsync(cancellationToken);
    }

    protected static IEnumerable<string> _keywordPatterns = new string[]{
        @"(^|\()\s*(?<keyword>select)\W",            // SELECT
        @"(^|\()\s*(?<keyword>select\s+distinct)\W", // SELECT DISTINCT
        @"\W(?<keyword>as)\W",                       // AS
        @"\W(?<keyword>from)\W",                     // FROM
        @"\W(?<keyword>cross\s+join)\W",             // CROSS JOIN
        @"\W(?<keyword>inner\s+join)\W",             // INNER JOIN
        @"\W(?<keyword>join)\W",                     // JOIN
        @"\W(?<keyword>left\s+outer\s+join)\W",      // LEFT OUTER JOIN
        @"\W(?<keyword>left\s+join)\W",              // LEFT JOIN
        @"\W(?<keyword>right\s+outer\s+join)\W",     // RIGHT OUTER JOIN
        @"\W(?<keyword>right\s+join)\W",             // RIGHT JOIN
        @"\W(?<keyword>full\s+outer\s+join)\W",      // FULL OUTER JOIN
        @"\W(?<keyword>outer\s+join)\W",             // OUTER JOIN
        @"\W(?<keyword>on)\W",                       // ON
        @"\W(?<keyword>where)\W",                    // WHERE
        @"\W(?<keyword>group\s+by)\W",               // GROUP BY
        @"\W(?<keyword>having)\W",                   // HAVING
        @"\W(?<keyword>order\s+by)\W",               // ORDER BY
        @"\W(?<keyword>asc)(\W|$)",                  // ASC
        @"\W(?<keyword>desc)(\W|$)",                 // DESC
        @"\W(?<keyword>and)\W",                      // AND
        @"\W(?<keyword>or)\W",                       // OR
        @"\W(?<keyword>not)\W",                      // NOT
        @"\W(?<keyword>all)\W",                      // ALL
        @"\W(?<keyword>any)\W",                      // ANY
        @"\W(?<keyword>some)\W",                     // SOME
        @"\W(?<keyword>between)\W",                  // BETWEEN
        @"\W(?<keyword>exists)(\W|$)",               // EXISTS
        @"\W(?<keyword>in)\W",                       // IN
        @"\W(?<keyword>like)\W",                     // LIKE
        @"\W(?<keyword>null)(\W|$)",                 // NULL
        @"\W(?<keyword>is\s+null)(\W|$)",            // IS NULL
        @"\W(?<keyword>is\s+not\s+null)(\W|$)",      // IS NOT NULL
    };

    protected static IEnumerable<string> _functionPatterns = new string[]{
        @"\W(?<function>max)\W",                     // MAX
        @"\W(?<function>min)\W",                     // MIN
    };

    protected abstract IEnumerable<string> KeywordPatterns { get; }
    protected abstract IEnumerable<string> FunctionPatterns { get; }
    public Regex KeywordsRx { get => _keywordsRx; }
    public Regex FunctionsRx { get => _functionsRx; }
    public Regex? TablesRx { get => _tablesRx; }
    public Regex? ColumnsRx { get => _columnsRx; }

    public IEnumerable<DbColumn>? GetColumns(string tableName) => _columns?[tableName];
    protected abstract Task<IEnumerable<DbColumn>> FetchColumnsAsync(string tableName, CancellationToken cancellationToken);
    private async Task CacheColumnsAsync(CancellationToken cancellationToken)
    {
        _columns = new Dictionary<string, IEnumerable<DbColumn>>();
        if (_tables is null)
        {
            return;
        }
        foreach (var table in _tables)
        {
            _columns.Add(table.Name, await FetchColumnsAsync(table.Name, cancellationToken));
        }
    }

    public IEnumerable<DbTable>? GetTables() => _tables;
    protected abstract Task<IEnumerable<DbTable>> FetchTablesAsync(CancellationToken cancellationToken);
    private async Task CacheTablesAsync(CancellationToken cancellationToken)
    {
        _tables = await FetchTablesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
        {
            return;
        }
        await _connection.DisposeAsync();
    }
}
