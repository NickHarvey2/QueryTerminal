using Microsoft.Data.SqlClient;

namespace QueryTerminal.Data;

public class SqlConnectionProvider : IDbConnectionProvider<SqlConnection>
{
    public SqlConnection Connect(string connectionString) => new SqlConnection(connectionString);
}
