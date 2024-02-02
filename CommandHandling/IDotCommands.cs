namespace QueryTerminal.CommandHandling;

public interface IDotCommands
{
    public Task OpenAsync(CancellationToken cancellationToken);
    public IEnumerable<IDotCommand> List();
    public IDotCommand Get(string commandName);
    public ValueTask DisposeAsync();
}
