using System.Reflection;

namespace QueryTerminal.OutputFormatting;

public record class OutputFormat(string Name, string Description, Func<IServiceProvider, object?, IOutputFormatter> ImplementationFactory)
{
    public static readonly OutputFormat Csv = new OutputFormat(
        Name: "csv", 
        Description: "Comma Separated Values, including headers",
        ImplementationFactory: (serviceProvider, serviceKey) => new DelimitedOutputFormatter(delimiter: ',', includeHeaders: true)
    );
    
    public static readonly OutputFormat CsvHeaders = new OutputFormat(
        Name: "csv-headers",
        Description: "Comma Separated Values, including headers", 
        ImplementationFactory: (serviceProvider, serviceKey) => new DelimitedOutputFormatter(delimiter: ',', includeHeaders: true)
    );
    
    public static readonly OutputFormat CsvNoheaders = new OutputFormat(
        Name: "csv-noheaders",
        Description: "Comma Separated Values, without headers", 
        ImplementationFactory: (serviceProvider, serviceKey) => new DelimitedOutputFormatter(delimiter: ',', includeHeaders: false)
    );
    
    public static readonly OutputFormat Tsv = new OutputFormat(
        Name: "tsv", 
        Description: "Tab Separated Values, including headers", 
        ImplementationFactory: (serviceProvider, serviceKey) => new DelimitedOutputFormatter(delimiter: '\t', includeHeaders: true)
    );
    
    public static readonly OutputFormat TsvHeaders = new OutputFormat(
        Name: "tsv-headers",
        Description: "Tab Separated Values, including headers", 
        ImplementationFactory: (serviceProvider, serviceKey) => new DelimitedOutputFormatter(delimiter: '\t', includeHeaders: true)
    );
    
    public static readonly OutputFormat TsvNoheaders = new OutputFormat(
        Name: "tsv-noheaders", 
        Description: "Tab Separated Values, without headers", 
        ImplementationFactory: (serviceProvider, serviceKey) => new DelimitedOutputFormatter(delimiter: '\t', includeHeaders: false)
    );
    
    public static readonly OutputFormat Json = new OutputFormat(
        Name: "json", 
        Description: "JSON format, minified and with no syntax highlights", 
        ImplementationFactory: (serviceProvider, serviceKey) => new JsonOutputFormatter(pretty: false)
    );
    
    public static readonly OutputFormat JsonMinified = new OutputFormat(
        Name: "json-minified", 
        Description: "JSON format, minified and with no syntax highlights", 
        ImplementationFactory: (serviceProvider, serviceKey) => new JsonOutputFormatter(pretty: false)
    );
    
    public static readonly OutputFormat JsonPretty = new OutputFormat(
        Name: "json-pretty", 
        Description: "JSON format, indented and with syntax highlights", 
        ImplementationFactory: (serviceProvider, serviceKey) => new JsonOutputFormatter(pretty: true)
    );
    
    public static readonly OutputFormat Yaml = new OutputFormat(
        Name: "yaml", 
        Description: "YAML format", 
        ImplementationFactory: (serviceProvider, serviceKey) => new YamlOutputFormatter()
    );
    
    public static readonly OutputFormat Table = new OutputFormat(
        Name: "table", 
        Description: "Table format using colors to improve readability", 
        ImplementationFactory: (serviceProvider, serviceKey) => new TableOutputFormatter(border: "square")
    );
    
    public static readonly OutputFormat Md = new OutputFormat(
        Name: "md", 
        Description: "Markdown table format, with no colors", 
        ImplementationFactory: (serviceProvider, serviceKey) => new TableOutputFormatter(border: "markdown")
    );
    
    public static readonly OutputFormat Markdown = new OutputFormat(
        Name: "markdown", 
        Description: "Markdown table format, with no colors", 
        ImplementationFactory: (serviceProvider, serviceKey) => new TableOutputFormatter(border: "markdown")
    );

    public static IEnumerable<OutputFormat?> List()
    {
        return typeof(OutputFormat).GetFields(BindingFlags.Public | BindingFlags.Static).Select(field => (OutputFormat)field.GetValue(null));
    }
}
