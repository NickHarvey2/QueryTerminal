using System.Data;

namespace QueryTerminal.OutputFormatting;

public interface IOutputFormatter
{
    void WriteOutput(IDataReader reader);
}
