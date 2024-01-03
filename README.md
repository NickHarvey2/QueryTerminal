# QueryTerminal

A simple tool for running SQL queries in the command line

## Usage
```
Usage:
  QueryTerminal [options]

Options:
  -c, --connectionString <connectionString>  The connection string used to connect to the SQL server.
  -q, --query <query>                        The SQL query to run. When using this option, the command is immediately run,
                                             output is sent to stdout, and the application terminates. 
                                             Omitting this option launches REPL mode.
  -o, --outputFormat <outputFormat>          The format to use to output data [default: csv]
  --version                                  Show version information
  -?, -h, --help                             Show help and usage information
```

## TODO
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
- unit tests
