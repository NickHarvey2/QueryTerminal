using System.CommandLine;
using QueryTerminal.CommandHandling;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using QueryTerminal.Data;
using QueryTerminal.Prompting;
using Microsoft.Extensions.Configuration;

namespace QueryTerminal;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // configure service collection
        var services = new ServiceCollection();

        services.AddKeyedTransient<IQueryTerminalDbConnection, SqlQueryTerminalDbConnection>("mssql");
        services.AddKeyedTransient<IQueryTerminalDbConnection, SqliteQueryTerminalDbConnection>("sqlite");

        services.AddTransient<DotCommandHandler>();

        services.AddTransient<QueryTerminalPrompt>();
        services.AddTransient<QueryTerminalPromptCallbacks>();

        services.AddSingleton<RootCommandHandler>();

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

        rootCommand.SetHandler(async context => {
            var dbType = context.ParseResult.GetValueForOption(databaseTypeOption);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddInMemoryCollection(
                rootCommand.Options.Select(option => new KeyValuePair<string, string?>(option.Name.Replace("--",""), context.ParseResult.GetValueForOption(option) as string))
            );
            services.AddSingleton<IConfiguration>(configurationBuilder.Build());
            var serviceProvider = services.BuildServiceProvider();
            
            var cancellationToken = context.GetCancellationToken();

            var handler = serviceProvider.GetRequiredService<RootCommandHandler>();
            await handler.RunAsync(cancellationToken);
        });

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
