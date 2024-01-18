using QueryTerminal.OutputFormatting;

namespace QueryTerminal.CommandHandling;

public interface IRootCommandHandler
{
    Task Run(CancellationToken cancellationToken);
    void Terminate();
    void SetOutputFormatByName(string outputFormatName);
    IOutputFormatter OutputFormatter { get; }
}
