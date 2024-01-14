using System.Reflection;

namespace QueryTerminal.OutputFormatting;

public static class OutputFormat
{
    public static readonly IOutputFormatter Csv = new DelimitedOutputFormatter(
        name: "csv",
        description: "Comma Separated Values, including headers",
        delimiter: ',',
        includeHeaders: true
    );

    public static readonly IOutputFormatter CsvHeaders = new DelimitedOutputFormatter(
        name: "csv-headers",
        description: "Comma Separated Values, including headers",
        delimiter: ',',
        includeHeaders: true
    );

    public static readonly IOutputFormatter CsvNoheaders = new DelimitedOutputFormatter(
        name: "csv-noheaders",
        description: "Comma Separated Values, without headers",
        delimiter: ',',
        includeHeaders: false
    );

    public static readonly IOutputFormatter Tsv = new DelimitedOutputFormatter(
        name: "tsv",
        description: "Tab Separated Values, including headers",
        delimiter: '\t',
        includeHeaders: true
    );

    public static readonly IOutputFormatter TsvHeaders = new DelimitedOutputFormatter(
        name: "tsv-headers",
        description: "Tab Separated Values, including headers",
        delimiter: '\t',
        includeHeaders: true
    );

    public static readonly IOutputFormatter TsvNoheaders = new DelimitedOutputFormatter(
        name: "tsv-noheaders",
        description: "Tab Separated Values, without headers",
        delimiter: '\t',
        includeHeaders: false
    );

    public static readonly IOutputFormatter Json = new JsonOutputFormatter(
        name: "json",
        description: "JSON format, minified and with no syntax highlights",
        pretty: false
    );

    public static readonly IOutputFormatter JsonMinified = new JsonOutputFormatter(
        name: "json-minified",
        description: "JSON format, minified and with no syntax highlights",
        pretty: false
    );

    public static readonly IOutputFormatter JsonPretty = new JsonOutputFormatter(
        name: "json-pretty",
        description: "JSON format, indented and with syntax highlights",
        pretty: true
    );

    public static readonly IOutputFormatter Yaml = new YamlOutputFormatter(
        name: "yaml",
        description: "YAML format"
    );

    public static readonly IOutputFormatter Table = new TableOutputFormatter(
        name: "table",
        description: "Table format using colors to improve readability",
        border: "square"
    );

    public static readonly IOutputFormatter Md = new TableOutputFormatter(
        name: "md",
        description: "Markdown table format, with no colors",
        border: "markdown"
    );

    public static readonly IOutputFormatter Markdown = new TableOutputFormatter(
        name: "markdown",
        description: "Markdown table format, with no colors",
        border: "markdown"
    );

    public static IEnumerable<IOutputFormatter?> List()
    {
        return typeof(OutputFormat).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (IOutputFormatter)field.GetValue(null));
    }

    public static IOutputFormatter? Get(string outputFormatName)
    {
        return List().Where(outputFormat => outputFormat.Name == outputFormatName).SingleOrDefault();
    }
}
