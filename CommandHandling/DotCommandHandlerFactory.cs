using System.Data.Common;
using QueryTerminal.Data;

namespace QueryTerminal.CommandHandling;

public class DotCommandHandlerFactory<TConnection> where TConnection : DbConnection
{
    private readonly IDbMetadataProvider<TConnection> _metadataProvider;

    public DotCommandHandlerFactory(IDbMetadataProvider<TConnection> metadataProvider)
    {
        _metadataProvider = metadataProvider;
    }

    public DotCommandHandler<TConnection> Create(RootCommandHandler rootCommandHandler, TConnection connection)
    {
        return new DotCommandHandler<TConnection>(rootCommandHandler, connection, _metadataProvider);
    }
}
