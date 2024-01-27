using PrettyPrompt;
using PrettyPrompt.Consoles;
using PrettyPrompt.Highlighting;

namespace QueryTerminal.Prompting;

public class QueryTerminalPrompt : IAsyncDisposable
{
    private readonly QueryTerminalPromptCallbacks _promptCallbacks;
    private readonly Prompt _delegatePrompt;

    public QueryTerminalPrompt(QueryTerminalPromptCallbacks promptCallbacks)
    {
        var persistentHistoryFilepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".queryterm.hist");
        _promptCallbacks = promptCallbacks;
        _delegatePrompt = new Prompt(
            persistentHistoryFilepath: persistentHistoryFilepath,
            callbacks: _promptCallbacks,
            configuration: new PromptConfiguration(
                prompt: "> ",
                keyBindings: new KeyBindings(
                    newLine: new KeyPressPatterns(new KeyPressPattern(ConsoleKey.Enter)),
                    submitPrompt: new KeyPressPatterns(
                        new KeyPressPattern(ConsoleModifiers.Control, ConsoleKey.Enter)
                    )
                ),
                // completionItemDescriptionPaneBackground: AnsiColor.Rgb(30, 30, 30),
                // selectedCompletionItemBackground: AnsiColor.Rgb(30, 30, 30),
                selectedTextBackground: AnsiColor.Rgb(20, 61, 102)
            )
        );
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

