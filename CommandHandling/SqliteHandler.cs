namespace QueryTerminal.CommandHandling;

public class SqliteHandler : BaseHandler
{
    public SqliteHandler(string? connectionString, IServiceProvider serviceProvider)
    {
        // if no connection string is provided, a transient in memory database may 
        // still be used, so no exception is thrown
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        SetOutputFormat("csv");
    }
}


