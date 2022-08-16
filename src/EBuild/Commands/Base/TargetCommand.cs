using System.ComponentModel;
using EBuild.Config.Resolved;
using EBuild.Consoles;
using EBuild.Yaml.Converters;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;

namespace EBuild.Commands.Base;

public class TargetSettings : ProjectSettings
{
    [CommandOption("-c|--concurrency")]
    [Description("并行任务个数。")]
    public int ConcurrencyCount { get; set; } = 1;

    [CommandArgument(0, "[构建目标]")]
    [Description("被构建的目标或源文件。")]
    public string[] Targets { get; init; } = Array.Empty<string>() ;
}

public abstract class TargetCommand<TSettings> : ProjectCommand<TSettings>
    where TSettings : TargetSettings
{
    public enum TargetStatus
    {
        [EnumAlias("等待中")] Waiting,

        [EnumAlias("[yellow]:red_exclamation_mark:[/]")]
        Skipped,
        [EnumAlias("进行中")] Doing,
        [EnumAlias("[green]:check_mark:[/]")] Done,
        [EnumAlias("[red]:cross_mark:[/]")] Error
    }

    protected enum WholeStatus
    {
        Doing,
        Completed,
        ErrorOccured
    }

    protected virtual int MaxConcurrencyCount => int.MaxValue;

    public TargetCommand(IDeserializer deserializer) : base(deserializer)
    {
    }

    protected abstract string GenerateHeading(WholeStatus status);

    protected virtual IMultiTaskDisplayer<ResolvedTarget, TargetStatus> GetDisplayer(IList<ResolvedTarget> targets)
    {
        return new TableDisplayer<ResolvedTarget, TargetStatus>(targets)
        {
            TargetName = target => target.Target.DisplayName(ProjectRoot),
            StatusString = e => EnumAliasAttribute.GetEnumAliasAttribute(e).Name
        };
    }

    protected sealed override async Task<int> OnExecuteInternalAsync(CancellationToken cancellationToken)
    {
        var possibleFile = CommandSettings.Targets.Select(x => Path.GetFullPath(x, ProjectRoot));

        var activatedTargets = _resolvedConfig.ResolveTargets
            .Where(x => CommandSettings.Targets.Length <= 0 || CommandSettings.Targets.Contains(x.Target.Name) ||
                        possibleFile.Contains(x.Target.Source))
            .ToList();

        if (activatedTargets.Count <= 0)
        {
            AnsiConsole.MarkupLine("[bold]没有找到符合要求的目标哦。[/]");
            return 0;
        }

        var displayer = GetDisplayer(activatedTargets);

        return await displayer.StartAsync(_ =>
            Task.FromResult(StartTargetTasks(activatedTargets, displayer, cancellationToken)));
    }

    private int StartTargetTasks(IList<ResolvedTarget> activatedTargets,
        IMultiTaskDisplayer<ResolvedTarget, TargetStatus> displayer,
        CancellationToken cancellationToken)
    {
        var c = Math.Min(CommandSettings.ConcurrencyCount, MaxConcurrencyCount);
        var semaphore = new Semaphore(c, c);
        var allOk = true;

        displayer.SetHeader(GenerateHeading(WholeStatus.Doing));

        var tasks = new List<Task>();
        for (int i = 0; i < activatedTargets.Count(); i++)
        {
            var activatedTarget = activatedTargets[i];

            var task = Task.Run(async () =>
            {
                var updateTargetStatus = displayer.GetUpdater(activatedTarget);
                updateTargetStatus(TargetStatus.Waiting, "正在等待...");

                if (await OnPreDoTargetAsync(activatedTarget, updateTargetStatus, cancellationToken))
                {
                    semaphore.WaitOne();
                    if (!await OnDoTargetAsync(activatedTarget, updateTargetStatus, cancellationToken))
                        allOk = false;
                    semaphore.Release();
                }
            }, cancellationToken);
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray(), cancellationToken);

        displayer.SetHeader(GenerateHeading(allOk ? WholeStatus.Completed : WholeStatus.ErrorOccured));
        return allOk ? 0 : 1;
    }

    protected abstract Task<bool> OnPreDoTargetAsync(ResolvedTarget target,
        Action<TargetStatus, string> updateTargetStatus,
        CancellationToken cancellationToken);

    protected abstract Task<bool> OnDoTargetAsync(ResolvedTarget target,
        Action<TargetStatus, string> updateTargetStatus,
        CancellationToken cancellationToken);
}