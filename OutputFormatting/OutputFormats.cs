using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;

namespace QueryTerminal.OutputFormatting;

public class OutputFormats : IOutputFormats
{
    private readonly IReadOnlyDictionary<string, IOutputFormatter> _outputFormatters;
    private IOutputFormatter _current;

    public OutputFormats(IRenderer renderer, IConfiguration configuration)
    {
        IOutputFormatter csv = new DelimitedOutputFormatter(
            renderer: renderer,
            name: "csv",
            description: "Comma Separated Values, including headers",
            delimiter: ',',
            includeHeaders: true
        );

        IOutputFormatter csvHeaders = new DelimitedOutputFormatter(
            renderer: renderer,
            name: "csv-headers",
            description: "Comma Separated Values, including headers",
            delimiter: ',',
            includeHeaders: true
        );

        IOutputFormatter csvNoheaders = new DelimitedOutputFormatter(
            renderer: renderer,
            name: "csv-noheaders",
            description: "Comma Separated Values, without headers",
            delimiter: ',',
            includeHeaders: false
        );

        IOutputFormatter tsv = new DelimitedOutputFormatter(
            renderer: renderer,
            name: "tsv",
            description: "Tab Separated Values, including headers",
            delimiter: '\t',
            includeHeaders: true
        );

        IOutputFormatter tsvHeaders = new DelimitedOutputFormatter(
            renderer: renderer,
            name: "tsv-headers",
            description: "Tab Separated Values, including headers",
            delimiter: '\t',
            includeHeaders: true
        );

        IOutputFormatter tsvNoheaders = new DelimitedOutputFormatter(
            renderer: renderer,
            name: "tsv-noheaders",
            description: "Tab Separated Values, without headers",
            delimiter: '\t',
            includeHeaders: false
        );

        IOutputFormatter wsv = new TableOutputFormatter(
            renderer: renderer,
            name: "wsv",
            description: "Whitespace Separated Values",
            border: "none"
        );

        IOutputFormatter wsvNoheaders = new TableOutputFormatter(
            renderer: renderer,
            name: "wsv-noheaders",
            description: "Whitespace Separated Values",
            border: "none",
            includeHeaders: false
        );

        IOutputFormatter json = new JsonOutputFormatter(
            renderer: renderer,
            name: "json",
            description: "JSON format, minified and with no syntax highlights",
            pretty: false
        );

        IOutputFormatter jsonMinified = new JsonOutputFormatter(
            renderer: renderer,
            name: "json-minified",
            description: "JSON format, minified and with no syntax highlights",
            pretty: false
        );

        IOutputFormatter jsonPretty = new JsonOutputFormatter(
            renderer: renderer,
            name: "json-pretty",
            description: "JSON format, indented and with syntax highlights",
            pretty: true
        );

        IOutputFormatter yaml = new YamlOutputFormatter(
            renderer: renderer,
            name: "yaml",
            description: "YAML format"
        );

        IOutputFormatter table = new TableOutputFormatter(
            renderer: renderer,
            name: "table",
            description: "Table format using colors to improve readability",
            border: "square"
        );

        IOutputFormatter md = new TableOutputFormatter(
            renderer: renderer,
            name: "md",
            description: "Markdown table format, with no colors",
            border: "markdown"
        );

        IOutputFormatter markdown = new TableOutputFormatter(
            renderer: renderer,
            name: "markdown",
            description: "Markdown table format, with no colors",
            border: "markdown"
        );

        _outputFormatters = ImmutableDictionary.CreateRange(
            new KeyValuePair<string, IOutputFormatter>[] {
                KeyValuePair.Create(csv.Name,          csv         ),
                KeyValuePair.Create(csvHeaders.Name,   csvHeaders  ),
                KeyValuePair.Create(csvNoheaders.Name, csvNoheaders),
                KeyValuePair.Create(tsv.Name,          tsv         ),
                KeyValuePair.Create(tsvHeaders.Name,   tsvHeaders  ),
                KeyValuePair.Create(tsvNoheaders.Name, tsvNoheaders),
                KeyValuePair.Create(wsv.Name,          wsv         ),
                KeyValuePair.Create(wsvNoheaders.Name, wsvNoheaders),
                KeyValuePair.Create(json.Name,         json        ),
                KeyValuePair.Create(jsonMinified.Name, jsonMinified),
                KeyValuePair.Create(jsonPretty.Name,   jsonPretty  ),
                KeyValuePair.Create(yaml.Name,         yaml        ),
                KeyValuePair.Create(table.Name,        table       ),
                KeyValuePair.Create(md.Name,           md          ),
                KeyValuePair.Create(markdown.Name,     markdown    ),
            }
        );

        _current = _outputFormatters[configuration["outputFormat"]];
    }

    public IEnumerable<IOutputFormatter> List { get => _outputFormatters.Values; }

    public IOutputFormatter Current { get => _current; set { _current = value; } }

    public IOutputFormatter this[string outputFormatName]
    {
        get {
            if (!_outputFormatters.ContainsKey(outputFormatName))
            {
                throw new ArgumentException($"Output Format Not Found: {outputFormatName}");
            }
            return _outputFormatters[outputFormatName];
        }
    }
}
