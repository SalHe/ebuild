using EBuild.Config.Resolved;
using EBuild.Consoles;
using EBuild.Yaml.Converters;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.Base;

[Command(
    "e2txt",
    Description = "将易语言代码转换到txt。",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect
)]
public abstract class TargetCommand : ProjectCommand
{
    protected enum TargetStatus
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

    public string[] RemainingArguments { get; }

    [Option("-c|--concurrency", Description = "并行任务个数。")]
    protected int ConcurrencyCount { get; set; } = 1;

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

    protected sealed override async Task<int> OnExecuteInternalAsync(CommandLineApplication application,
        CancellationToken cancellationToken)
    {
        var activatedTargets = _resolvedConfig.ResolveTargets
            .Where(x => RemainingArguments.Length <= 0 || RemainingArguments.Contains(x.Target.Name))
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
        var c = Math.Min(ConcurrencyCount, MaxConcurrencyCount);
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