using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using EBuild.Commands.Base;
using EBuild.Config;
using EBuild.Sources;
using EBuild.Toolchain;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Command("run", Description = "运行脚本或易语言源文件。", ExtendedHelpText = @"当第一个参数对应了配置文件中某一脚本名字时，将执行脚本对应的内容；
当第一个参数对应了某一个源码的文件名时，将编译该易语言源文件并执行(要求源文件必须是Windows可执行程序或Windows控制台程序，由于需要编译，建议源文件尽可能小)。

由于执行的对象可能是脚本或者易语言源文件，所以建议您为脚本命名的时候不要与源文件重名。
此外，目前ebuild不会保证到底先执行哪种情况，所以更加建议您不要重名。

当您执行易语言源文件时，请您自行确保源文件的安全性。",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect,
    AllowArgumentSeparator = true)]
public class RunCommand : ProjectCommand
{
    private readonly EclToolchain _eclToolchain;
    protected override IEnumerable<IToolchain> NeededToolchains { get; }

    [Argument(0, "脚本名称或源文件。")] [Required] public string TargetToRun { get; set; }

    [Option("--verbose", Description = "输出编译源文件的输出日志。")]
    public bool Verbose { get; set; } = false;

    public string[] RemainingArguments { get; }

    public RunCommand(IDeserializer deserializer, IEnumerable<IToolchain> toolchains, EclToolchain eclToolchain) :
        base(deserializer)
    {
        NeededToolchains = toolchains;
        _eclToolchain = eclToolchain;
    }

    protected override async Task<int> OnExecuteInternalAsync(CommandLineApplication application,
        CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(TargetToRun);
        var exeOrBatPath = "";
        if (File.Exists(fullPath))
        {
            AnsiConsole.WriteLine("[yellow]源文件{0}存在，将尝试编译该文件并运行。[/]", fullPath);
            var meta = ESourceMeta.FromSource(fullPath);
            if (meta == null || meta.SourceType == SourceType.ECom ||
                (meta.TargetType != TargetType.WinConsole && meta.TargetType != TargetType.WinForm))
            {
                AnsiConsole.MarkupLine("[red]您必须提供一个有效的易语言源文件，并且该源码应编译成Windows控制台程序或Windows窗口程序。[/]");
                return 1;
            }

            exeOrBatPath = await QuickBuildESource(
                cancellationToken, _eclToolchain,
                (status, log) => Console.WriteLine(log),
                fullPath, meta);
            if (!string.IsNullOrEmpty(exeOrBatPath))
                AnsiConsole.MarkupLine("[yellow]编译成功，准备运行：{0}[/]", Markup.Escape(exeOrBatPath));
        }
        else if (_resolvedConfig.RootConfig.Scripts?.ContainsKey(TargetToRun) == true)
        {
            var tempFile = Path.GetTempFileName();
            var batContent = _resolvedConfig.RootConfig.Scripts[TargetToRun];
            batContent = batContent.ReplaceLineEndings("\r\n");
            await File.WriteAllTextAsync(tempFile, batContent, cancellationToken);
            exeOrBatPath = Path.ChangeExtension(tempFile, "ebuild-run.bat");
            File.Move(tempFile, exeOrBatPath);

            AnsiConsole.MarkupLine("[yellow]准备运行：{0}[/]", Markup.Escape(exeOrBatPath));
        }
        else
        {
            AnsiConsole.MarkupLine("[red]找不到对于的脚本({0})或源码({1})[/]", Markup.Escape(fullPath),
                Markup.Escape(TargetToRun));
            return 1;
        }

        Console.WriteLine();

        var process = new Process();
        foreach (var arg in RemainingArguments) process.StartInfo.ArgumentList.Add(arg);
        process.StartInfo.FileName = exeOrBatPath;
        // process.StartInfo.EnvironmentVariables // TODO 加入环境变量
        process.StartInfo.RedirectStandardError
            = process.StartInfo.RedirectStandardOutput
                = process.StartInfo.RedirectStandardInput
                    = true;
        process.Start();

        cancellationToken.Register(() =>
        {
            process.Kill();
            File.Delete(exeOrBatPath);
        });

        var tasks = new List<Task>()
        {
            ReadStreamBuffered(process.StandardOutput, Console.Out),
            ReadStreamBuffered(process.StandardError, Console.Error),
            ReadStreamBuffered(new StreamReader(Console.OpenStandardInput()), process.StandardInput)
        };
        await process.WaitForExitAsync(cancellationToken);

        async Task ReadStream(TextReader tr, TextWriter tw)
        {
            var buffer = new char[1024];
            while (!process.HasExited)
            {
                var s = await tr.ReadToEndAsync();
                if (!string.IsNullOrEmpty(s))
                    await tw.WriteAsync(s);
            }
        }

        async Task ReadStreamBuffered(TextReader tr, TextWriter tw)
        {
            var buffer = new char[1024];
            while (!process.HasExited)
            {
                var len = await tr.ReadAsync(buffer, cancellationToken);
                if (len > 0) await tw.WriteAsync(buffer, 0, len);
            }
        }

        await Console.Out.WriteAsync(await process.StandardOutput.ReadToEndAsync());
        await Console.Error.WriteAsync(await process.StandardError.ReadToEndAsync());

        File.Delete(exeOrBatPath);
        return process.ExitCode;
    }

    private static async Task<string> QuickBuildESource(CancellationToken cancellationToken, EclToolchain eclToolchain,
        Action<TargetCommand.TargetStatus, string>? updateStatus, string fullPath, ESourceMeta meta)
    {
        var tempFile = Path.GetTempFileName();
        var tempExe = Path.ChangeExtension(tempFile, "ebuild-run.exe");
        File.Delete(tempFile);

        var process = new Process();
        foreach (var arg in EclToolchain.Args(fullPath, tempExe, meta, Compiler.Normal, "", "", "", false))
            process.StartInfo.ArgumentList.Add(arg);
        process.StartInfo.FileName = eclToolchain.ExecutablePath;
        if (updateStatus != null)
        {
            var handler = BuildCommand.GetEclLogHandler(updateStatus, () => tempExe = "");
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += handler;
            process.ErrorDataReceived += handler;
        }

        cancellationToken.Register(() =>
        {
            process.CancelErrorRead();
            process.CancelOutputRead();
            process.Kill();
            File.Delete(tempExe);
            tempExe = "";
        });

        process.Start();
        if (updateStatus != null)
        {
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
        }

        await process.WaitForExitAsync(cancellationToken);

        return fullPath;
    }
}