using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using EBuild.Commands.Base;
using EBuild.Config;
using EBuild.Plugins;
using EBuild.Project;
using EBuild.Sources;
using EBuild.Toolchain;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Description(@"运行脚本或易语言源文件。

当第一个参数对应了配置文件中某一脚本名字时，将执行脚本对应的内容；
当第一个参数对应了某一个源码的文件名时，将编译该易语言源文件并执行(要求源文件必须是Windows可执行程序或Windows控制台程序，由于需要编译，建议源文件尽可能小)。

由于执行的对象可能是脚本或者易语言源文件，所以建议您为脚本命名的时候不要与源文件重名。
此外，目前ebuild不会保证到底先执行哪种情况，所以更加建议您不要重名。

当您执行易语言源文件时，请您自行确保源文件的安全性。")]
public class RunCommand : ProjectCommand<RunCommand.Settings>
{
    public class Settings : ProjectSettings
    {
        [CommandArgument(0, "<脚本名称|易语言源文件>")]
        [Required]
        public string TargetToRun { get; init; }

        [CommandOption("--verbose")]
        [Description("输出编译源文件的输出日志。")]
        [DefaultValue(false)]
        public bool Verbose { get; init; }
    }

    private readonly EclToolchain _eclToolchain;
    private readonly EnvironmentVariables _environmentVariables;
    protected override IEnumerable<IToolchain> NeededToolchains { get; }

    public RunCommand(IDeserializer deserializer, IEnumerable<IToolchain> toolchains, EclToolchain eclToolchain,
        EnvironmentVariables environmentVariables, IEnumerable<IPlugin> plugins) :
        base(deserializer,plugins)
    {
        NeededToolchains = toolchains;
        _eclToolchain = eclToolchain;
        _environmentVariables = environmentVariables;
    }

    protected override async Task<int> OnExecuteInternalAsync(CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(CommandSettings.TargetToRun);
        var exeOrBatPath = "";
        if (File.Exists(fullPath))
        {
            AnsiConsole.MarkupLine("[yellow]源文件`{0}`存在，将尝试编译该文件并运行。[/]", fullPath);
            var meta = ESourceMeta.FromSource(fullPath);
            if (meta == null || meta.SourceType == SourceType.ECom ||
                (meta.TargetType != TargetType.WinConsole && meta.TargetType != TargetType.WinForm))
            {
                AnsiConsole.MarkupLine("[red]您必须提供一个有效的易语言源文件，并且该源码应编译成Windows控制台程序或Windows窗口程序。[/]");
                return 1;
            }

            exeOrBatPath = await QuickBuildESource(
                cancellationToken, _eclToolchain,
                CommandSettings.Verbose,
                fullPath, meta);
            if (!string.IsNullOrEmpty(exeOrBatPath))
                AnsiConsole.MarkupLine("[yellow]编译成功，准备运行：`{0}`[/]", Markup.Escape(exeOrBatPath));
        }
        else if (_resolvedConfig.RootConfig.Scripts?.ContainsKey(CommandSettings.TargetToRun) == true)
        {
            var tempFile = Path.GetTempFileName();
            var batContent = _resolvedConfig.RootConfig.Scripts[CommandSettings.TargetToRun];
            batContent = batContent.ReplaceLineEndings("\r\n");
            await File.WriteAllTextAsync(tempFile, batContent, cancellationToken);
            exeOrBatPath = Path.ChangeExtension(tempFile, "ebuild-run.bat");
            File.Move(tempFile, exeOrBatPath);

            AnsiConsole.MarkupLine("[yellow]准备运行：`{0}`[/]", Markup.Escape(exeOrBatPath));
        }
        else
        {
            AnsiConsole.MarkupLine("[red]找不到对于的脚本({0})或源码({1})[/]", Markup.Escape(fullPath),
                Markup.Escape(CommandSettings.TargetToRun));
            return 1;
        }

        var process = new Process();
        foreach (var arg in CommandContext.Remaining.Raw) process.StartInfo.ArgumentList.Add(arg);
        process.StartInfo.FileName = exeOrBatPath;

        _environmentVariables.ForProject(ProjectRoot, _resolvedConfig.OutputDir);
        _environmentVariables.LoadToStringDictionary(process.StartInfo.EnvironmentVariables);

        process.Start();

        cancellationToken.Register(() =>
        {
            process.Kill();
            File.Delete(exeOrBatPath);
        });
        await process.WaitForExitAsync(cancellationToken);

        process.Kill();

        File.Delete(exeOrBatPath);
        return process.ExitCode;
    }

    private static async Task<string> QuickBuildESource(CancellationToken cancellationToken, EclToolchain eclToolchain,
        bool verbose, string fullPath, ESourceMeta meta)
    {
        var tempFile = Path.GetTempFileName();
        var tempExe = Path.ChangeExtension(tempFile, "ebuild-run.exe");
        File.Delete(tempFile);

        var process = new Process();
        foreach (var arg in EclToolchain.Args(fullPath, tempExe, meta, Compiler.Normal, "", "", "", false))
            process.StartInfo.ArgumentList.Add(arg);
        process.StartInfo.FileName = eclToolchain.ExecutablePath;
        if (!verbose)
        {
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
        }

        cancellationToken.Register(() =>
        {
            process.Kill();
            File.Delete(tempExe);
            tempExe = "";
        });
        process.Start();
        await process.WaitForExitAsync(cancellationToken);
        return tempExe;
    }
}