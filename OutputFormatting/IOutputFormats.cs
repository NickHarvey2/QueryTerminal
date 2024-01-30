namespace QueryTerminal.OutputFormatting;

public interface IOutputFormats
{
    IEnumerable<IOutputFormatter?> List();
    IOutputFormatter? Get(string outputFormatName);
}
