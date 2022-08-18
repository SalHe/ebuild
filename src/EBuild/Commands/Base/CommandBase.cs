using EBuild.Plugins;
using Spectre.Console.Cli;

namespace EBuild.Commands.Base;

public class GeneralSettings : CommandSettings
{
}

public class CommandBase<TSettings> : AsyncCommand<TSettings>
    where TSettings : GeneralSettings
{
    public CommandBase(IEnumerable<IPlugin> plugins)
    {
        _plugins = plugins;
    }

    public TSettings CommandSettings { get; private set; }
    public CommandContext CommandContext { get; set; }
    protected IEnumerable<IPlugin> _plugins;

    public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Token.Register(() => Console.CancelKeyPress -= Handler);

        void Handler(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            cts.Cancel();
        }

        Console.CancelKeyPress += Handler;

        CommandSettings = settings;
        CommandContext = context;

        return await OnExecuteAsync(cts.Token);
    }

    public virtual Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }
}