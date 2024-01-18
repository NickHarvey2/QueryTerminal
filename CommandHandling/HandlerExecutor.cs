namespace QueryTerminal.CommandHandling;

public delegate Task HandlerExecutor(RootCommandHandler handler, CancellationToken cancellationToken);
