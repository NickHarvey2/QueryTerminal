using System.Data;

namespace QueryTerminal.OutputFormatting;

public interface IOutputFormatter
{
    string Name { get; }
    string Description { get; }
    void WriteOutput(IDataReader reader);
}
