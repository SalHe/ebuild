using System.ComponentModel;
using EBuild.Commands.Base;
using EBuild.Project.Cleaners;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Description(@"清理恢复的源码等工程中间文件。
该命令可以清理恢复的源码等工程中间文件。包括：

1. txt2e 中从文本格式代码恢复出来的易语言源文件(*.recover.e，只清理包含在工程源文件和目标中的恢复代码)；
2. e2txt 中从易语言源文件转换成的文本格式代码的文件夹(*.ecode，只清理包含在工程源文件和目标中的恢复代码)。（可选，默认不清理）
3. build 中构建出来的目标文件（实际会清理构建输出文件夹中的所有文件）。
")]
public class CleanCommand : ProjectCommand<CleanCommand.Settings>
{
    public class Settings : ProjectSettings
    {
        [CommandOption("--ecode")]
        [Description("清理 .ecode 文件夹")]
        public bool CleanECode { get; init; } = false;

        [CommandOption("-f|--force")]
        [Description("强制同意。针对危险的操作，ebuild会尝试询问您是否确定，但是如果您开启了此标志，则ebuild认为您坚持对所有危险操作继续执行。")]
        public bool ForceClean { get; init; } = false;
    }

    private readonly IEnumerable<ProjectCleaner> _projectCleaners;

    public CleanCommand(IDeserializer deserializer, IEnumerable<ProjectCleaner> projectCleaners) : base(deserializer)
    {
        _projectCleaners = projectCleaners;
    }

    protected override Task<int> OnExecuteInternalAsync(CancellationToken token)
    {
        var activatedCleaners = _projectCleaners.Where(x => x is not ProjectECodeCleaner || CommandSettings.CleanECode);
        foreach (var cleaner in activatedCleaners)
        {
            AnsiConsole.MarkupLine($"[green]正在清理 {Markup.Escape(cleaner.CleanContent)}[/]");
            if (!CommandSettings.ForceClean && cleaner.Optional &&
                !AnsiConsole.Confirm($"即将清理 [red]{Markup.Escape(cleaner.CleanContent)}[/]，这可能会使您的数据丢失，是否确认清理？", false))
            {
                AnsiConsole.MarkupLine("[yellow]您已放弃清理[/]");
                AnsiConsole.WriteLine();
                continue;
            }

            foreach (var target in _resolvedConfig.ResolveTargets)
            {
                try
                {
                    cleaner.Clean(target, _resolvedConfig);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (cleaner.Once) break;
            }

            Console.WriteLine();
        }

        return Task.FromResult(0);
    }
}