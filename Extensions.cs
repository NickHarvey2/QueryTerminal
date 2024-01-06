using System.CommandLine;

namespace QueryTerminal;

public static class Extensions
{  
    public static void SetHandler<T1,T2,T3>(this Command command, Func<CancellationToken,IServiceProvider,T1?,T2?,T3?,Task> handler, IServiceProvider serviceProvider, params Option[] options)
    {
        command.SetHandler(async context => {
            object?[] optionValues = options.Select(o => context.ParseResult.GetValueForOption(o)).ToArray();
            CancellationToken cancellationToken = context.GetCancellationToken();
            await handler(cancellationToken, serviceProvider, (T1?)optionValues[0], (T2?)optionValues[1], (T3?)optionValues[2]);
        });
    }

    public static void SetHandler<T1,T2,T3,T4>(this Command command, Func<CancellationToken,IServiceProvider,T1?,T2?,T3?,T4?,Task> handler, IServiceProvider serviceProvider, params Option[] options)
    {
        command.SetHandler(async context => {
            object?[] optionValues = options.Select(o => context.ParseResult.GetValueForOption(o)).ToArray();
            CancellationToken cancellationToken = context.GetCancellationToken();
            await handler(cancellationToken, serviceProvider, (T1?)optionValues[0], (T2?)optionValues[1], (T3?)optionValues[2], (T4?)optionValues[3]);
        });
    }
}
