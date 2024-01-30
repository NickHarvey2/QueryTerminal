using Spectre.Console;
using Spectre.Console.Rendering;

namespace QueryTerminal.OutputFormatting;

public class Renderer : IRenderer
{
    public void Render(string output)
    {
        Console.WriteLine(output);
    }

    public void Render(IRenderable output)
    {
        AnsiConsole.Write(output);
    }
}
