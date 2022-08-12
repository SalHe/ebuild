using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using EBuild.Commands.Base;
using EBuild.Config.Resolved;
using EBuild.Project;
using EBuild.Toolchain;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

public class E2Txt : TargetCommand
{
    private readonly E2TxtToolchain _e2txt;

    public E2Txt(IDeserializer deserializer, E2TxtToolchain e2txt) : base(deserializer)
    {
        _e2txt = e2txt;
    }

    protected override IEnumerable<IToolchain> NeededToolchains => new IToolchain[] { _e2txt };

    protected override string GenerateHeading(WholeStatus status)
    {
        switch (status)
        {
            case WholeStatus.Doing:
                return "e2txt";
            case WholeStatus.Completed:
                return "[green]:check_mark:e2txt[/]";
            case WholeStatus.ErrorOccured:
                return "[red]:cross_mark:e2txt[/]";
        }

        return "";
    }

    private static readonly Regex _multispaces = new Regex(@"\s+");

    protected override async Task<bool> OnDoTargetAsync(ResolvedTarget target, UpdateTargetStatus updateTargetStatus,
        CancellationToken cancellationToken)
    {
        updateTargetStatus(TargetStatus.Doing, "开始转换");

        var noError = true;
        var convertOk = false;

        var args = GetArgs(target);
        updateTargetStatus(TargetStatus.Doing,
            string.Format("[grey]{0}[/]", Markup.Escape(_e2txt.ExecutablePath + " " + string.Join(" ", args))));

        var process = new Process();
        foreach (var s in args) process.StartInfo.ArgumentList.Add(s);
        process.StartInfo.FileName = _e2txt.ExecutablePath;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("gbk");
        process.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("gbk");

        var error = "";

        var d = (DataReceivedEventHandler)((sender, eventArgs) =>
        {
            if (!string.IsNullOrEmpty(eventArgs.Data))
            {
                if (eventArgs.Data.StartsWith("SUCC:"))
                {
                    // 成功
                    convertOk = true;
                    var outputDir = eventArgs.Data.Substring(5);
                    updateTargetStatus(TargetStatus.Done,
                        string.Format("[green]转换成功：{0}[/]",
                            Markup.Escape(outputDir)
                        )
                    );
                }
                else if (eventArgs.Data.StartsWith("ERROR:"))
                {
                    // 错误提示
                    noError = false;
                    error += eventArgs.Data.Substring(6) + "\n";
                    updateTargetStatus(TargetStatus.Error,
                        string.Format("[red]转换出错：{0}[/]",
                            Markup.Escape(_multispaces.Replace(error, " "))
                        )
                    );
                }
                else
                {
                    // 日志
                    if (!convertOk)
                        updateTargetStatus(noError ? TargetStatus.Doing : TargetStatus.Error,
                            Markup.Escape(_multispaces.Replace(eventArgs.Data, " ")));
                }
            }
        });
        process.OutputDataReceived += d;
        process.ErrorDataReceived += d;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        cancellationToken.Register(() =>
        {
            process.Kill();
            process.CancelOutputRead();;
            process.CancelErrorRead();;
        });
        await process.WaitForExitAsync(cancellationToken);

        if (!noError)
        {
            updateTargetStatus(TargetStatus.Error,
                string.Format("[red]转换过程中出现错误：\n{0}[/]",
                    Markup.Escape(error)
                )
            );
        }

        return convertOk;
    }

    protected virtual IList<string> GetArgs(ResolvedTarget target)
    {
        return E2TxtToolchain.E2TxtArgs(target.Target.Source, target.Target.GetECodeDir(),
            _resolvedConfig.RootConfig.E2Txt);
    }

    protected override Task<bool> OnPreDoTargetAsync(ResolvedTarget target, UpdateTargetStatus updateTargetStatus,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(target.Password))
        {
            updateTargetStatus(TargetStatus.Waiting, "等待转换中...");
            return Task.FromResult(true);
        }

        updateTargetStatus(TargetStatus.Skipped, "[yellow]源文件存在密码，已跳过转换！[/]");
        return Task.FromResult(false);
    }
}