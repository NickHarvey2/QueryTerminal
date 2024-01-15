using System.Data.Common;
using QueryTerminal.Data;

namespace QueryTerminal.CommandHandling;

public class DotCommandHandlerFactory<TConnection> where TConnection : DbConnection, new()
{
    public DotCommandHandler<TConnection> Create(RootCommandHandler rootCommandHandler, QueryTerminalDbConnection<TConnection> connection)
    {
        return new DotCommandHandler<TConnection>(rootCommandHandler, connection);
    }
}
