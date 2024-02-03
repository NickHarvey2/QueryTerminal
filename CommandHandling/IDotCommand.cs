namespace QueryTerminal.CommandHandling;

public interface IDotCommand
{
    string Name { get; }
    string Description { get; }
    bool Invoke(params string[] args);
}
