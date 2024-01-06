using Microsoft.Data.Sqlite;

namespace QueryTerminal.Data;

public class SqliteConnectionProvider : IDbConnectionProvider<SqliteConnection>
{
    public SqliteConnection Connect(string connectionString) => new SqliteConnection(connectionString);
}

