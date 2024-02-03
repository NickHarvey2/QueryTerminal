namespace QueryTerminal.OutputFormatting;

public interface IOutputFormats
{
    IEnumerable<IOutputFormatter> List { get; }
    IOutputFormatter Current { get; set; }
    IOutputFormatter this[string outputFormatName] { get; }
}
