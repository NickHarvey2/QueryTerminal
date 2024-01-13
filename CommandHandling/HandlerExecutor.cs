namespace QueryTerminal.CommandHandling;

public delegate Task HandlerExecutor(RootCommandHandler handler, string? sqlQuery, CancellationToken cancellationToken);
