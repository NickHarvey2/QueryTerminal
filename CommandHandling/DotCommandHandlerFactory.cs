using System.Data.Common;
using QueryTerminal.Data;

namespace QueryTerminal.CommandHandling;

public class DotCommandHandlerFactory<TConnection> where TConnection : DbConnection, new()
{
    private readonly QueryTerminalDbConnection<TConnection> _connection;

    public DotCommandHandlerFactory(QueryTerminalDbConnection<TConnection> connection)
    {
        _connection = connection;
    }

    public DotCommandHandler<TConnection> Create(RootCommandHandler rootCommandHandler)
    {
        return new DotCommandHandler<TConnection>(rootCommandHandler, _connection);
    }
}
