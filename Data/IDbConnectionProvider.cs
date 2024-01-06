using System.Data;

namespace QueryTerminal.Data;

public interface IDbConnectionProvider<TConnection> where TConnection : IDbConnection
{
    TConnection Connect(string connectionString);
}

