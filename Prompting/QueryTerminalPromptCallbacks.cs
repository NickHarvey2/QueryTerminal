using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrettyPrompt;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;
using QueryTerminal.Data;

namespace QueryTerminal.Prompting;

public class QueryTerminalPromptCallbacks : PromptCallbacks, IAsyncDisposable
{
    private readonly IQueryTerminalDbConnection _connection;
    private readonly static AnsiColor _keywordColor = AnsiColor.Blue;
    private readonly static AnsiColor _numericLiteralColor = AnsiColor.Magenta;
    private readonly static AnsiColor _stringLiteralColor = AnsiColor.Green;

    private readonly static Regex _stringLiteralRx = new Regex(
        @"(\W(?<string_literal>'(?:[^']|'')*')(\W|$))|(\W(?<string_literal>'(?:[^']|'')*)$)",
        RegexOptions.IgnoreCase 
        | RegexOptions.CultureInvariant 
        | RegexOptions.ExplicitCapture 
        | RegexOptions.Compiled
    );

    private readonly static Regex _numericLiteralRx = new Regex(
        @"(?<!')\b(?<num_literal>\d+)\b(?!')",
        // @"\W(?<num_literal>\d+)(\W|$)", 
        RegexOptions.IgnoreCase 
        | RegexOptions.CultureInvariant 
        | RegexOptions.ExplicitCapture 
        | RegexOptions.Compiled
    );

    public QueryTerminalPromptCallbacks(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _connection = serviceProvider.GetRequiredKeyedService<IQueryTerminalDbConnection>(configuration["type"]);
    }

    // TODO: Reverse search
    protected override IEnumerable<(KeyPressPattern Pattern, KeyPressCallbackAsync Callback)> GetKeyPressCallbacks()
    {
        // registers functions to be called when the user presses a key. The text
        // currently typed into the prompt, along with the caret position within
        // that text are provided as callback parameters.
        yield return (new(ConsoleModifiers.Control, ConsoleKey.R), (text, caret, cancellationToken) => {
            Console.Beep();
            return Task.FromResult<KeyPressCallbackResult?>(null);
        });
    }

    protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
    {
        var typedWord = text.AsSpan(spanToBeReplaced.Start, spanToBeReplaced.Length).ToString();
        return Task.FromResult<IReadOnlyList<CompletionItem>>(Enumerable.Empty<CompletionItem>().ToImmutableList());
    }

    protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
    {
        var keywordFormatSpans = _connection.KeywordsRx.Matches(text).Select(match => {
            var keyword = match.Groups["keyword"];
            return new FormatSpan(keyword.Index, keyword.Length, _keywordColor);
        });

        var numericLiteralFormatSpans = _numericLiteralRx.Matches(text).Select(match => {
            var keyword = match.Groups["num_literal"];
            return new FormatSpan(keyword.Index, keyword.Length, _numericLiteralColor);
        });

        var stringLiteralFormatSpans = _stringLiteralRx.Matches(text).Select(match => {
            var keyword = match.Groups["string_literal"];
            return new FormatSpan(keyword.Index, keyword.Length, _stringLiteralColor);
        });

        return Task.FromResult<IReadOnlyCollection<FormatSpan>>(
            keywordFormatSpans
            .Concat(numericLiteralFormatSpans)
            .Concat(stringLiteralFormatSpans)
            .ToImmutableList()
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
