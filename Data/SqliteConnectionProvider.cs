using Microsoft.Data.Sqlite;

namespace QueryTerminal.Data;

public class SqliteConnectionProvider : IDbConnectionProvider<SqliteConnection>
{
    private SqliteExtensionProvider _extensionProvider;

    public SqliteConnectionProvider(SqliteExtensionProvider extensionProvider)
    {
        _extensionProvider = extensionProvider;
    }

    public SqliteConnection Connect(string connectionString)
    {
        var connection = new SqliteConnection(connectionString);
        foreach (var extension in _extensionProvider.GetExtensions())
        {
            connection.LoadExtension(extension);
        }
        return connection;
    }
}

