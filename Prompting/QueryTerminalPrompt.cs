using PrettyPrompt;
using PrettyPrompt.Consoles;
using PrettyPrompt.Highlighting;

namespace QueryTerminal.Prompting;

public class QueryTerminalPrompt : IAsyncDisposable
{
    private readonly static string _persistentHistoryFilepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".queryterm.hist");

    private readonly static KeyBindings _keyBindings = new KeyBindings(
        newLine:      new KeyPressPatterns(new KeyPressPattern(ConsoleKey.Enter)),
        submitPrompt: new KeyPressPatterns(new KeyPressPattern(ConsoleModifiers.Control, ConsoleKey.Enter))
    );

    private readonly static PromptConfiguration _promptConfiguration = new PromptConfiguration(
        prompt: "> ",
        keyBindings: _keyBindings,
        selectedTextBackground: AnsiColor.Rgb(20, 61, 102)
    );

    private readonly QueryTerminalPromptCallbacks _promptCallbacks;
    private readonly Prompt _delegatePrompt;

    public QueryTerminalPrompt(QueryTerminalPromptCallbacks promptCallbacks)
    {
        _promptCallbacks = promptCallbacks;
        _delegatePrompt = new Prompt(
            persistentHistoryFilepath: _persistentHistoryFilepath,
            callbacks: _promptCallbacks,
            configuration: _promptConfiguration
        );
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await _promptCallbacks.OpenAsync(cancellationToken);
    }

    public async Task<PromptResult> ReadLineAsync()
    {
        return await _delegatePrompt.ReadLineAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _delegatePrompt.DisposeAsync();
        await _promptCallbacks.DisposeAsync();
    }
}

