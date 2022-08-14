using EBuild.Commands.Base;
using EBuild.Config.Resolved;
using EBuild.Consoles;
using EBuild.Toolchain;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Command("build", Description = "构建目标。可指定需要构建的目标。如`ebuild build 程序1 程序2`。")]
public class BuildCommand : TargetCommand
{
    private readonly EclToolchain _eclToolchain;

    public BuildCommand(IDeserializer deserializer, EclToolchain eclToolchain) : base(deserializer)
    {
        _eclToolchain = eclToolchain;
    }

    protected override IEnumerable<IToolchain> NeededToolchains => new IToolchain[] { _eclToolchain };
    protected override int MaxConcurrencyCount => 1; // 如果多任务并行编译可能会影响 ecl 获取易语言窗口

    protected override IMultiTaskDisplayer<ResolvedTarget, TargetStatus> GetDisplayer(IList<ResolvedTarget> targets)
    {
        var displayer = new LinesDisplayer<ResolvedTarget, TargetStatus>()
        {
            TargetName = target => target.Target.DisplayName(ProjectRoot),
            StatusString = e =>
            {
                switch (e)
                {
                    case TargetStatus.Waiting:
                        return "[grey]{0}[/]";
                    case TargetStatus.Skipped:
                        return "[yellow]{0}[/]";
                    case TargetStatus.Doing:
                        return "{0}";
                    case TargetStatus.Done:
                        return "[green]{0}[/]";
                    case TargetStatus.Error:
                        return "[red]{0}[/]";
                }

                return "{0}";
            }
        };
        if (ConcurrencyCount > MaxConcurrencyCount)
            displayer.SetHeader("[grey]目前不支持并行编译目标[/]");
        return displayer;
    }

    protected override string GenerateHeading(WholeStatus status)
    {
        switch (status)
        {
            case WholeStatus.Doing:
                return "[green]正在构建目标...[/]";
            case WholeStatus.Completed:
                return "[green]:check_mark: 完成构建[/]";
            case WholeStatus.ErrorOccured:
                return "[red]:cross_mark: 构建过程中出现错误[/]";
        }

        return "";
    }

    protected override Task<bool> OnPreDoTargetAsync(ResolvedTarget target,
        Action<TargetStatus, string> updateTargetStatus,
        CancellationToken cancellationToken)
    {
        var args = EclToolchain.Args(target, _resolvedConfig.OutputDir);
        updateTargetStatus(TargetStatus.Waiting,
            $"准备编译中... {Markup.Escape(_eclToolchain.ExecutablePath)} {Markup.Escape(string.Join(" ", args))}");

        return Task.FromResult(true);
    }

    protected override async Task<bool> OnDoTargetAsync(ResolvedTarget target,
        Action<TargetStatus, string> updateTargetStatus,
        CancellationToken cancellationToken)
    {
        updateTargetStatus(TargetStatus.Doing, "开始编译");
        await Task.Delay(500, cancellationToken); // TODO 实现ecl编译
        updateTargetStatus(TargetStatus.Done, "完成编译");
        return true;
    }
}