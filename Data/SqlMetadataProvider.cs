using Microsoft.Data.SqlClient;

namespace QueryTerminal.Data;

public class SqlMetadataProvider : IDbMetadataProvider<SqlConnection>
{
    public async Task<IEnumerable<DbColumn>> GetColumns(SqlConnection connection, string tableName, CancellationToken cancellationToken)
    {
        var commandText = $"SELECT COLUMN_NAME,DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{tableName}'";
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

    public async Task<IEnumerable<DbTable>> GetTables(SqlConnection connection, CancellationToken cancellationToken)
    {
        var commandText = "SELECT TABLE_NAME,TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
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
