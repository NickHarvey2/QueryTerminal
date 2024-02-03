namespace QueryTerminal.CommandHandling;

public class DotCommmand : IDotCommand
{
    private readonly string _name;
    private readonly string _description;
    private readonly Func<string[], bool> _command;

    public string Name { get => _name; }
    public string Description { get => _description; }

    public DotCommmand(string name, string description, Func<string[], bool> command)
    {
        _name = name;
        _description = description;
        _command = command;
    }

    public bool Invoke(params string[] args)
    {
        return _command(args);
    }
}
