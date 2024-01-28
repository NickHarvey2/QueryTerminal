using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace QueryTerminal.Data;

public class SqliteQueryTerminalDbConnection : QueryTerminalDbConnection<SqliteConnection>
{
    public SqliteQueryTerminalDbConnection(IConfiguration configuration) : base(configuration) { }

    public override async Task OpenAsync(CancellationToken cancellationToken)
    {
        await base.OpenAsync(cancellationToken);
        // Load extensions here
    }
    
    private new static IEnumerable<string> _keywordPatterns = new string[]{
        @"\W(?<keyword>limit)\W",         // LIMIT
    };
    private new static IEnumerable<string> _functionPatterns = new string[]{
        @"\W(?<function>substr)\W",       // SUBSTR
        @"\W(?<function>length)\W",       // LENGTH
        @"\W(?<function>group_concat)\W", // GROUP_CONCAT
    };

    protected override IEnumerable<string> KeywordPatterns { get => _keywordPatterns.Concat(QueryTerminalDbConnection<SqliteConnection>._keywordPatterns); }
    protected override IEnumerable<string> FunctionPatterns { get => _functionPatterns.Concat(QueryTerminalDbConnection<SqliteConnection>._functionPatterns); }
    
    protected override async Task<IEnumerable<DbColumn>> FetchColumnsAsync(string tableName, CancellationToken cancellationToken)
    {
        var commandText = $"SELECT name,type FROM PRAGMA_TABLE_INFO('{tableName}')";
        var command = _connection.CreateCommand();
        command.CommandText = commandText;
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var columns = new List<DbColumn>();
        while (reader.Read())
        {
            object[] row = new object[reader.FieldCount];
            var count = reader.GetValues(row);
            columns.Add(new DbColumn(Name: row[0].ToString(), Type: row[1].ToString()));
        }
        return columns;
    }

    protected override async Task<IEnumerable<DbTable>> FetchTablesAsync(CancellationToken cancellationToken)
    {
        var commandText = "SELECT name,type FROM sqlite_schema WHERE name NOT LIKE 'sqlite_%'";
        var command = _connection.CreateCommand();
        command.CommandText = commandText;
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var tables = new List<DbTable>();
        while (reader.Read())
        {
            object[] row = new object[reader.FieldCount];
            var count = reader.GetValues(row);
            tables.Add(new DbTable(Name: row[0].ToString(), Type: row[1].ToString()));
        }
        return tables;
    }
}
