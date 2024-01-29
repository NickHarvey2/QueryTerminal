using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace QueryTerminal.Data;

public class SqlQueryTerminalDbConnection : QueryTerminalDbConnection<SqlConnection>
{
    public SqlQueryTerminalDbConnection(IConfiguration configuration) : base(configuration) { }
    
    private new static IEnumerable<string> _keywordPatterns = new string[]{
        @"\W(?<keyword>top)\W",           // TOP
    };
    private new static IEnumerable<string> _functionPatterns = new string[]{
        @"\W(?<function>substring)\W",    // SUBSTRING
        @"\W(?<function>len)\W",          // LEN
        @"\W(?<function>string_agg)\W",   // STRING_AGG
    };

    protected override IEnumerable<string> KeywordPatterns { get => _keywordPatterns.Concat(QueryTerminalDbConnection<SqlConnection>._keywordPatterns); }
    protected override IEnumerable<string> FunctionPatterns { get => _functionPatterns.Concat(QueryTerminalDbConnection<SqlConnection>._functionPatterns); }

    protected override async Task<IEnumerable<DbColumn>> FetchColumnsAsync(string tableName, CancellationToken cancellationToken)
    {
        var commandText = $"SELECT COLUMN_NAME,DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{tableName}'";
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
        var commandText = "SELECT TABLE_NAME,TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
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
