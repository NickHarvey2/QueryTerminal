using PrettyPrompt;
using PrettyPrompt.Highlighting;

namespace QueryTerminal.Prompting;

public class QueryTerminalPrompt : IAsyncDisposable
{
    private readonly Prompt _delegatePrompt;

    public QueryTerminalPrompt()
    {
        var persistentHistoryFilepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".queryterm.hist");
        _delegatePrompt = new Prompt(
            persistentHistoryFilepath: persistentHistoryFilepath,
            callbacks: new QueryTerminalPromptCallbacks(),
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
    }
}

