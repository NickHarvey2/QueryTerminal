using Microsoft.Data.Sqlite;

namespace QueryTerminal.Data;

public class SqliteMetadataProvider : IDbMetadataProvider<SqliteConnection>
{
    public async Task<IEnumerable<DbColumn>> GetColumns(SqliteConnection connection, string tableName, CancellationToken cancellationToken)
    {
        var commandText = $"SELECT name,type FROM PRAGMA_TABLE_INFO('{tableName}')";
        var command = connection.CreateCommand();
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

    public async Task<IEnumerable<DbTable>> GetTables(SqliteConnection connection, CancellationToken cancellationToken)
    {
        var commandText = "SELECT name,type FROM sqlite_schema WHERE name NOT LIKE 'sqlite_%'";
        var command = connection.CreateCommand();
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
