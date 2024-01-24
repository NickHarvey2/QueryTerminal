using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace QueryTerminal.Data;

public abstract class QueryTerminalDbConnection<TConnection> : IQueryTerminalDbConnection where TConnection : DbConnection, new()
{
    protected TConnection _connection;
    private readonly Regex _keywordsRx;

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
    }

    public virtual async Task OpenAsync(CancellationToken cancellationToken)
    {
        await _connection.OpenAsync(cancellationToken);
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
        @"(^|\()\s*(?<keyword>as)\W",                // AS
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
    protected abstract IEnumerable<string> KeywordPatterns { get; }
    public Regex KeywordsRx { get => _keywordsRx; }

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
