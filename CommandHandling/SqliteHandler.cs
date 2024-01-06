namespace QueryTerminal.CommandHandling;

public class SqliteHandler : BaseHandler
{
    public SqliteHandler(string? connectionString, IServiceProvider serviceProvider)
    {
        _connectionString = connectionString;
        _serviceProvider = serviceProvider;
        SetOutputFormat("csv");
    }
}


