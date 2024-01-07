namespace QueryTerminal.CommandHandling;

public class SqlHandler : BaseHandler
{
    public SqlHandler(string? connectionString, IServiceProvider serviceProvider)
    {
        // if no connection string is provided, no operations can occur
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException("connectionString");
        }
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        SetOutputFormat("csv");
    }
}

