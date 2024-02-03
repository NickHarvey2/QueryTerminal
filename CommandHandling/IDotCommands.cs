namespace QueryTerminal.CommandHandling;

public interface IDotCommands
{
    Task OpenAsync(CancellationToken cancellationToken);
    IEnumerable<IDotCommand> List { get; }
    IDotCommand this[string commandName] { get; }
    ValueTask DisposeAsync();
}
