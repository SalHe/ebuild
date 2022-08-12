using EBuild.Config.Resolved;
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
    public virtual int ConcurrencyCount { get; set; } = 1;

    protected delegate void UpdateTargetStatus(TargetStatus status, string log);

    public TargetCommand(IDeserializer deserializer) : base(deserializer)
    {
    }

    protected abstract string GenerateHeading(WholeStatus status);

    protected sealed override int OnExecuteInternal(CommandLineApplication application)
    {
        var table = new Table();
        var activedTargets = _resolvedConfig.ResolveTargets
            .Where(x => RemainingArguments.Length <= 0 || RemainingArguments.Contains(x.Target.Name))
            .ToList();

        if (activedTargets.Count <= 0)
        {
            AnsiConsole.MarkupLine("[bold]没有找到符合要求的目标哦。[/]");
            return 0;
        }

        return AnsiConsole.Live(table).Start(ctx => StartTargetTasks(activedTargets, ctx, table));
    }

    private int StartTargetTasks(IList<ResolvedTarget> activedTargets, LiveDisplayContext ctx, Table table)
    {
        var semaphore = new Semaphore(ConcurrencyCount, ConcurrencyCount);
        var allOk = true;

        table.Title(GenerateHeading(WholeStatus.Doing));
        table.AddColumns("状态", "目标", "日志");

        var tasks = new List<Task>();
        foreach (var activedTarget in activedTargets)
            table.AddRow("", Markup.Escape(activedTarget.Target.DisplayName(ProjectRoot)), "");
        for (int i = 0; i < activedTargets.Count(); i++)
        {
            var activedTarget = activedTargets[i];

            var capturedI = i;
            var task = Task.Run(async () =>
            {
                var updateTargetStatus = (UpdateTargetStatus)((status, log) =>
                {
                    table.UpdateCell(capturedI, 0, new Markup(EnumAliasAttribute.GetEnumAliasAttribute(status)!.Name));
                    table.UpdateCell(capturedI, 2, new Markup(log));
                    ctx.Refresh();
                });
                updateTargetStatus(TargetStatus.Waiting, "正在等待...");

                if (await OnPreDoTarget(activedTarget, updateTargetStatus))
                {
                    semaphore.WaitOne();
                    if (!await OnDoTarget(activedTarget, updateTargetStatus))
                        allOk = false;
                    semaphore.Release();
                }
            });
            tasks.Add(task);
        }

        Task.WaitAll(tasks.ToArray());

        if (allOk)
            table.Title(GenerateHeading(WholeStatus.Completed));
        else
            table.Title(GenerateHeading(WholeStatus.ErrorOccured));
        Console.Clear();
        ctx.Refresh();
        return allOk ? 0 : 1;
    }

    protected abstract Task<bool> OnPreDoTarget(ResolvedTarget target, UpdateTargetStatus updateTargetStatus);

    protected abstract Task<bool> OnDoTarget(ResolvedTarget target, UpdateTargetStatus updateTargetStatus);
}