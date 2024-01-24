using PrettyPrompt;
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

