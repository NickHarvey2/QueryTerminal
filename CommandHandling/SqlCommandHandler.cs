using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.Data;

namespace QueryTerminal.CommandHandling;

public class SqlCommandHandler : BaseCommandHandler<SqlConnection>
{
    public override IQueryExecutor<SqlConnection> QueryExecutor => _serviceProvider.GetRequiredService<IQueryExecutor<SqlConnection>>();

    public override SqlConnection Connect(string connectionString) => _serviceProvider.GetRequiredService<IDbConnectionProvider<SqlConnection>>().Connect(connectionString);

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

