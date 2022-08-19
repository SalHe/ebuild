using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using EBuild.Commands.Base;
using EBuild.Config.Resolved;
using EBuild.Consoles;
using EBuild.Hooks;
using EBuild.Plugins;
using EBuild.Project;
using EBuild.Toolchain;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Description(@"构建目标。

本命令帮助您编译您配置文件的中的构建目标以及被搜索到的源文件。
您也可以手动指定要编译的目标或者源文件，如`ebuild build 程序1 程序2`。
当您不指定编译目标的时候，ebuild默认编译所有目标。")]
public class BuildCommand : TargetCommand<TargetSettings>
{
    private readonly EnvironmentVariables _environmentVariables;
    private readonly EclToolchain _eclToolchain;

    public BuildCommand(IDeserializer deserializer, IEnumerable<IPlugin> plugins,
        EnvironmentVariables environmentVariables, EclToolchain eclToolchain) : base(deserializer, plugins)
    {
        _environmentVariables = environmentVariables;
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
        if (CommandSettings.ConcurrencyCount > MaxConcurrencyCount)
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

    protected override async Task<bool> OnPreDoTargetAsync(ResolvedTarget target,
        Action<TargetStatus, string> updateTargetStatus,
        CancellationToken cancellationToken)
    {
        if (target.ShouldBuild)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(target.Target.OutputPath(_resolvedConfig.OutputDir))!);

            var args = GetBuildArgs(target, true);
            updateTargetStatus(TargetStatus.Waiting,
                $"准备编译中... {Markup.Escape(_eclToolchain.ExecutablePath)} {Markup.Escape(string.Join(" ", args))}");
        }
        else
        {
            updateTargetStatus(TargetStatus.Skipped, "本目标已被排除编译，将被跳过");
        }

        if (target.ShouldBuild)
        {
            var evs = (EnvironmentVariables)_environmentVariables.Clone();
            SetupEnvironmentVariables(target, evs, Hook.PreBuild);
            foreach (var plugin in _plugins)
            {
                bool shouldContinue = await plugin.OnHook(new PluginContext(_resolvedConfig, evs)
                {
                    BuildTarget = target,
                    UpdateStatus = (status, log) => updateTargetStatus(status, $"[yellow][[{Hook.PreBuild}]]{log}[/]"),
                }, cancellationToken, Hook.PreBuild);
                if (!shouldContinue)
                {
                    updateTargetStatus(TargetStatus.Skipped, $"[[{Hook.PreBuild}]]已由构建周期脚本取消该任务！");
                    return false;
                }
            }
        }

        return target.ShouldBuild;
    }

    private void SetupEnvironmentVariables(ResolvedTarget target, EnvironmentVariables evs, Hook hook)
    {
        evs.ForProject(ProjectRoot, _resolvedConfig.OutputDir);
        evs.AddRange(new[]
        {
            new EnvironmentVariable("EBUILD_PERIOD", "构建生命周期", hook.ToString),
            new EnvironmentVariable("EBUILD_SOURCE_FILE", "被构建的源文件", () => target.Target.Source),
            new EnvironmentVariable("EBUILD_TARGET_FILE", "构建目标输出路径",
                () => target.Target.OutputPath(_resolvedConfig.OutputDir)),
            new EnvironmentVariable("EBUILD_TARGET_TYPE", "构建目标类型",
                () => target.SourceMeta?.TargetType.ToString() ?? "(Unkown)"),
        });
    }

    private IList<string> GetBuildArgs(ResolvedTarget target, bool hideSecret = false)
    {
        return EclToolchain.Args(target, _resolvedConfig.OutputDir, hideSecret);
    }

    protected override async Task<bool> OnDoTargetAsync(ResolvedTarget target,
        Action<TargetStatus, string> updateTargetStatus,
        CancellationToken cancellationToken)
    {
        updateTargetStatus(TargetStatus.Doing, "开始编译");

        var compileOk = true;
        var handler = GetEclLogHandler(Update, () => compileOk = false);

        var process = new Process();
        foreach (var arg in GetBuildArgs(target)) process.StartInfo.ArgumentList.Add(arg);
        process.StartInfo.FileName = _eclToolchain.ExecutablePath;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardOutputEncoding =
            process.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("gbk");
        process.OutputDataReceived += handler;
        process.ErrorDataReceived += handler;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        cancellationToken.Register(() => process.Kill());
        await process.WaitForExitAsync(cancellationToken);

        void Update(TargetStatus status, string log)
        {
            if (!string.IsNullOrEmpty(log))
            {
                updateTargetStatus(status, Markup.Escape(log));
            }
        }

        var evs = (EnvironmentVariables)_environmentVariables.Clone();
        SetupEnvironmentVariables(target, evs, Hook.PostBuild);
        foreach (var plugin in _plugins)
        {
            await plugin.OnHook(new PluginContext(_resolvedConfig, evs)
            {
                BuildTarget = target,
                UpdateStatus = (status, log) => updateTargetStatus(status, $"[yellow][[{Hook.PostBuild}]]{log}[/]"),
            }, cancellationToken, Hook.PostBuild);
        }

        if (compileOk)
            Update(TargetStatus.Done, $"编译成功，保存到：{target.Target.OutputPath(_resolvedConfig.OutputDir)}");

        return compileOk;
    }

    public static DataReceivedEventHandler GetEclLogHandler(Action<TargetStatus, string> update, Action compileFailed)
    {
        return (sender, e) =>
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            if (!EclToolchain.TryMatchError(e.Data, out bool isOk, out int errorCode, out string tip))
            {
                update(TargetStatus.Doing, e.Data);
                return;
            }

            if (!isOk)
            {
                update(TargetStatus.Error, $"编译目标出错，错误代码：{errorCode}，错误提示：{tip}");
                compileFailed();
            }
        };
    }
}