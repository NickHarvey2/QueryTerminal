# QueryTerminal

A simple tool for running SQL queries in the command line

## Usage
```
Usage:
  QueryTerminal [options]

Options:
  -t, --type <type>                          The type of database to connect to. Supported values are 'mssql' for Microsoft SQL Server and 'sqlite' for SQLite.
  -c, --connectionString <connectionString>  The connection string used to connect to the database.
  -q, --query <query>                        The query to run. When using this option, the command is immediately run, output is sent to stdout, and the application terminates. Omitting this option launches REPL mode.
  -o, --outputFormat <outputFormat>          The format to use to output data [default: csv]
  --version                                  Show version information
  -?, -h, --help                             Show help and usage information -?, -h, --help                             Show help and usage information
```

## TODO
- unit tests
- SQLite engine
    - Loading Extensions, in particular to enable use of SteamPipe modules that can be loaded as SQLite extensions providing virtual tables
- dot commands
    - .listTables
    - .listColumns {table}
    - .setOutputFormat {format}
    - .getOutputFormat
    - .listOutputFormats
    - .listNamedQueries
    - .execNamedQuery {namedQuery} [parameters ...]
- loading config from file
    - connection string / DB type
    - parameterized query list
- output formatter framework
    - `useQuotes` option for tsv/csv
    - breakdown chart for spectre tables
- REPL mode
    - completion
    - multi line mode
    - command history
- Multiple named connections, allowing joins across connections
