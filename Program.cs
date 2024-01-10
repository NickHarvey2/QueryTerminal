using System.CommandLine;
using Microsoft.Data.SqlClient;
using QueryTerminal.CommandHandling;
using Microsoft.Extensions.DependencyInjection;
using QueryTerminal.OutputFormatting;
using Spectre.Console;
using Microsoft.Data.Sqlite;
using QueryTerminal.Data;

namespace QueryTerminal;

class Program
{
    private delegate Task HandlerExecutor(RootCommandHandler handler, string sqlQuery, CancellationToken cancellationToken);

    static async Task<int> Main(string[] args)
    {
        // configure service collection
        var services = new ServiceCollection();

        // Runners
        services.AddKeyedTransient<HandlerExecutor>("mssql", (serviceProvider,serviceKey) => (handler,sqlQuery,cancellationToken) => handler.Run<SqlConnection>(sqlQuery,cancellationToken));
        services.AddKeyedTransient<HandlerExecutor>("sqlite", (serviceProvider,serviceKey) => (handler,sqlQuery,cancellationToken) => handler.Run<SqliteConnection>(sqlQuery,cancellationToken));

        // Connection providers
        services.AddTransient<IDbConnectionProvider<SqlConnection>, SqlConnectionProvider>();
        services.AddTransient<IDbConnectionProvider<SqliteConnection>, SqliteConnectionProvider>();

        // Query executors
        services.AddTransient<IQueryExecutor<SqlConnection>, SqlQueryExecutor>();
        services.AddTransient<IQueryExecutor<SqliteConnection>, SqliteQueryExecutor>();

        // Output formatters
        services.AddKeyedTransient<IOutputFormatter>("csv",           (serviceProvider,serviceKey) => new DelimitedOutputFormatter(delimiter: ',', includeHeaders: true));
        services.AddKeyedTransient<IOutputFormatter>("csv-headers",   (serviceProvider,serviceKey) => new DelimitedOutputFormatter(delimiter: ',', includeHeaders: true));
        services.AddKeyedTransient<IOutputFormatter>("csv-noheaders", (serviceProvider,serviceKey) => new DelimitedOutputFormatter(delimiter: ',', includeHeaders: false));
        services.AddKeyedTransient<IOutputFormatter>("tsv",           (serviceProvider,serviceKey) => new DelimitedOutputFormatter(delimiter: '\t', includeHeaders: true));
        services.AddKeyedTransient<IOutputFormatter>("tsv-headers",   (serviceProvider,serviceKey) => new DelimitedOutputFormatter(delimiter: '\t', includeHeaders: true));
        services.AddKeyedTransient<IOutputFormatter>("tsv-noheaders", (serviceProvider,serviceKey) => new DelimitedOutputFormatter(delimiter: '\t', includeHeaders: false));
        services.AddKeyedTransient<IOutputFormatter>("json",          (serviceProvider,serviceKey) => new JsonOutputFormatter(pretty: false));
        services.AddKeyedTransient<IOutputFormatter>("json-minified", (serviceProvider,serviceKey) => new JsonOutputFormatter(pretty: false));
        services.AddKeyedTransient<IOutputFormatter>("json-pretty",   (serviceProvider,serviceKey) => new JsonOutputFormatter(pretty: true));
        services.AddKeyedTransient<IOutputFormatter>("yaml",          (serviceProvider,serviceKey) => new YamlOutputFormatter());
        services.AddKeyedTransient<IOutputFormatter>("table",         (serviceProvider,serviceKey) => new TableOutputFormatter(border: "square"));
        services.AddKeyedTransient<IOutputFormatter>("md",            (serviceProvider,serviceKey) => new TableOutputFormatter(border: "markdown"));
        services.AddKeyedTransient<IOutputFormatter>("markdown",      (serviceProvider,serviceKey) => new TableOutputFormatter(border: "markdown"));

        var serviceProvider = services.BuildServiceProvider();

        // Configure command and options
        var rootCommand = new RootCommand("Connect and run commands against databases");

        var databaseTypeOption = new Option<string>(
            name: "--type",
            description: "The type of database to connect to. Supported values are 'mssql' for Microsoft SQL Server and 'sqlite' for SQLite."
        );
        databaseTypeOption.AddAlias("-t");
        rootCommand.AddOption(databaseTypeOption);

        var connectionStringOption = new Option<string?>(
            name: "--connectionString",
            description: "The connection string used to connect to the database."
        );
        connectionStringOption.AddAlias("-c");
        rootCommand.AddOption(connectionStringOption);

        var sqlQueryOption = new Option<string?>(
            name: "--query",
            description: "The query to run. When using this option, the command is immediately run, output is sent to stdout, and the application terminates. Omitting this option launches REPL mode."
        );
        sqlQueryOption.AddAlias("-q");
        rootCommand.AddOption(sqlQueryOption);

        var outputFormatOption = new Option<string>(
            name: "--outputFormat",
            description: "The format to use to output data",
            getDefaultValue: () => "csv"
        );
        outputFormatOption.AddAlias("-o");
        rootCommand.AddOption(outputFormatOption);

        rootCommand.SetHandler<string,string,string,string>(async (cancellationToken, serviceProvider, dbType, connectionString, sqlQuery, outputFormat) => {
            var handler = new RootCommandHandler(connectionString, serviceProvider);
            handler.SetOutputFormat(outputFormat);
            var executor = serviceProvider.GetRequiredKeyedService<HandlerExecutor>(dbType);
            await executor.Invoke(handler, sqlQuery, cancellationToken);
        }, serviceProvider, databaseTypeOption, connectionStringOption, sqlQueryOption, outputFormatOption);

        // Execution and error handling
        try {
            await rootCommand.InvokeAsync(args);
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Aborted");
            return 1;
        }
        catch (ArgumentException e)
        {
            AnsiConsole.WriteException(e);
            return 1;
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
            return 1;
        }

        return 0;
    }
}
