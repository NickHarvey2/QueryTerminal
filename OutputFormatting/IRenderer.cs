using Spectre.Console.Rendering;

namespace QueryTerminal.OutputFormatting;

public interface IRenderer
{
    void Render(string output);
    void Render(IRenderable output);
}
