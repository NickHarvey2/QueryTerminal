namespace QueryTerminal.CommandHandling;

public class SqliteHandler : BaseHandler
{
    public SqliteHandler(string? connectionString, IServiceProvider serviceProvider)
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


