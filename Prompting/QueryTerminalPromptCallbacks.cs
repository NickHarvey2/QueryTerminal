using System.Collections.Immutable;
using System.Text.RegularExpressions;
using PrettyPrompt;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;
using QueryTerminal.CommandHandling;
using QueryTerminal.Data;
using QueryTerminal.OutputFormatting;

namespace QueryTerminal.Prompting;

public class QueryTerminalPromptCallbacks : PromptCallbacks, IAsyncDisposable
{
    private readonly IQueryTerminalDbConnection _connection;
    private readonly IDotCommands _dotCommands;
    private readonly IOutputFormats _outputFormats;
    // private readonly StreamWriter _fileStream;
    private readonly static AnsiColor _keywordColor        = AnsiColor.Magenta;
    private readonly static AnsiColor _numericLiteralColor = AnsiColor.Rgb(255,177,0);
    private readonly static AnsiColor _stringLiteralColor  = AnsiColor.Green;
    private readonly static AnsiColor _functionColor       = AnsiColor.Yellow;
    private readonly static AnsiColor _tableColor          = AnsiColor.Blue;
    private readonly static AnsiColor _columnColor         = AnsiColor.Cyan;

    private readonly static Regex _stringLiteralRx = new Regex(
        @"(\W(?<string_literal>'(?:[^']|'')*')(\W|$))|(\W(?<string_literal>'(?:[^']|'')*)$)",
        RegexOptions.IgnoreCase 
        | RegexOptions.CultureInvariant 
        | RegexOptions.ExplicitCapture 
        | RegexOptions.Compiled
    );

    private readonly static Regex _numericLiteralRx = new Regex(
        @"(?<!')\b(?<num_literal>\d+)\b(?!')",
        RegexOptions.IgnoreCase
        | RegexOptions.CultureInvariant
        | RegexOptions.ExplicitCapture
        | RegexOptions.Compiled
    );

    public QueryTerminalPromptCallbacks(IQueryTerminalDbConnection connection, IDotCommands dotCommands, IOutputFormats outputFormats)
    {
        _connection = connection;
        _dotCommands = dotCommands;
        _outputFormats = outputFormats;
        // _fileStream = new StreamWriter(File.Create("debug.log"));
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await _connection.OpenAsync(cancellationToken);
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

    protected override async Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
    {
        // await _fileStream.WriteLineAsync($"{text}|{caret}|{spanToBeReplaced.Start}|{spanToBeReplaced.End}|{spanToBeReplaced.ToString()}");
        // await _fileStream.FlushAsync();
        // var typedWord = text.AsSpan(spanToBeReplaced.Start, spanToBeReplaced.Length).ToString();
        if (text == ".")
        {
            return _dotCommands.List.Select(dotCommand => new CompletionItem(
                replacementText: dotCommand.Name.Substring(1),
                displayText: dotCommand.Name,
                getExtendedDescription: _ => Task.FromResult(new FormattedString(dotCommand.Description))
            )).ToImmutableList();
        }
        if (text.Length >= 12 && text.Substring(0,12) == ".listColumns")
        {
            return _connection.GetTables().Select(table => new CompletionItem(
                replacementText: table.Name,
                displayText: table.Name
            )).ToImmutableList();
        }
        if (text.Length >= 16 && text.Substring(0,16) == ".setOutputFormat")
        {
            return _outputFormats.List.Select(outputFormat => new CompletionItem(
                replacementText: outputFormat.Name,
                displayText: outputFormat.Name,
                getExtendedDescription: _ => Task.FromResult(new FormattedString(outputFormat.Description))
            )).ToImmutableList();
        }
        return Enumerable.Empty<CompletionItem>().ToImmutableList();
    }

    protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(string text, CancellationToken cancellationToken)
    {
        // Depending on if I find I need it, memoization might be useful to optimize this
        // because the vast majority of changes to `text` are going to be by a single character, often at the end of the string
        // so if we cache the results and only recalculate the FormatSpans that intersect a changed region, that will save us some
        // operations -- then a again, regex, especially compiled, is probably one of the most thoroughly optimized things
        // in any given language, so probably unnecessary as long as highlights are regex based, so a memoization implementation
        // could actually make performance _worse_
        var functionFormatSpans = _connection.FunctionsRx.Matches(text).Select(match => {
            var function = match.Groups["function"];
            return new FormatSpan(function.Index, function.Length, _functionColor);
        });

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

        var tableFormatSpans = _connection.TablesRx?.Matches(text).Select(match => {
            var function = match.Groups["table"];
            return new FormatSpan(function.Index, function.Length, _tableColor);
        }) ?? Enumerable.Empty<FormatSpan>();

        var columnFormatSpans = _connection.ColumnsRx?.Matches(text).Select(match => {
            var function = match.Groups["column"];
            return new FormatSpan(function.Index, function.Length, _columnColor);
        }) ?? Enumerable.Empty<FormatSpan>();

        return Task.FromResult<IReadOnlyCollection<FormatSpan>>(
            keywordFormatSpans
            .Concat(functionFormatSpans)
            .Concat(numericLiteralFormatSpans)
            .Concat(stringLiteralFormatSpans)
            .Concat(tableFormatSpans)
            .Concat(columnFormatSpans)
            .ToImmutableList()
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
