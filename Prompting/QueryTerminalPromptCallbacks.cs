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
    protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<CompletionItem>>(Enumerable.Empty<CompletionItem>().ToImmutableList());
    }

    protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<FormatSpan>>(EnumerateFormatSpans(text).ToImmutableList());
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
        "SELECT ",
        "FROM ",
        "WHERE ",
        "AS ",
        "GROUP BY ",
        "ORDER BY ",
        " DESC",
        " ASC",
        "IN ",
        "AND ",
        "OR ",
        "NOT ",
        "LIKE ",
        "INNER JOIN ",
        "JOIN ",
        "LEFT OUTER JOIN ",
        "LEFT JOIN ",
        "RIGHT OUTER JOIN ",
        "RIGHT JOIN ",
        "FULL OUTER JOIN ",
        "OUTER JOIN ",
        " ON ",
        "(",
        ")",
        "=",
        "!=",
    };

}
