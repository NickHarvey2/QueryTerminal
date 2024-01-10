using System.Data.Common;

namespace QueryTerminal.Data;

public interface IDbConnectionProvider<TConnection> where TConnection : DbConnection
{
    TConnection Connect(string connectionString);
}

