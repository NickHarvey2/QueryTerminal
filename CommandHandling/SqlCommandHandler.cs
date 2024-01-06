namespace QueryTerminal.CommandHandling;

public class SqlCommandHandler : BaseCommandHandler
{
    public SqlCommandHandler(string? connectionString, IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException("connectionString");
        }
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        SetOutputFormat("csv");
    }
}

