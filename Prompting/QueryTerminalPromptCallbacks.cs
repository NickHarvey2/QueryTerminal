using System.Collections.Immutable;
using PrettyPrompt;
using PrettyPrompt.Completion;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;

namespace QueryTerminal.Prompting;

internal class QueryTerminalPromptCallbacks : PromptCallbacks
{
    // TODO: Reverse search
    // protected override IEnumerable<(KeyPressPattern Pattern, KeyPressCallbackAsync Callback)> GetKeyPressCallbacks()
    // {
    //     // registers functions to be called when the user presses a key. The text
    //     // currently typed into the prompt, along with the caret position within
    //     // that text are provided as callback parameters.
    //     yield return (new(ConsoleModifiers.Control, ConsoleKey.R), ReverseCommandSearch);
    // }

    // TODO: implement this stub
    protected override async Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
    {
        return Enumerable.Empty<CompletionItem>().ToImmutableList();
    }

    protected override async Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
    {
        return EnumerateFormatSpans(text).ToImmutableList();
    }

    private static IEnumerable<FormatSpan> EnumerateFormatSpans(string text)
    {
        foreach (var textToFormat in Keywords)
        {
            int startIndex;
            int offset = 0;
            while ((startIndex = text.AsSpan(offset).IndexOf(textToFormat, StringComparison.InvariantCultureIgnoreCase)) != -1)
            {
                yield return new FormatSpan(offset + startIndex, textToFormat.Length, AnsiColor.Blue);
                offset += startIndex + textToFormat.Length;
            }
        }
    }

    private static readonly string[] Keywords = new[]
    {
        "SELECT",
        "FROM",
        "AS",
        "GROUP BY",
        "ORDER BY",
        "DESC",
        "ASC",
        "IN",
        "AND",
        "OR",
        "NOT",
        "LIKE",
        "(",
        ")",
        "=",
    };

}
