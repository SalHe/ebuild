using System.Diagnostics;
using System.Text;
using EBuild.Commands.Base;
using EBuild.Hooks;
using Spectre.Console;

namespace EBuild.Plugins;

public class BuildHookScriptPlugin : Plugin
{
    public override async Task<bool> OnHook(PluginContext context, CancellationToken cancellationToken, Hook hook)
    {
        string batContent = context.BuildTarget!.Target?.Hooks?[hook].ReplaceLineEndings("\r\n") ?? "";
        if (string.IsNullOrEmpty(batContent))
            return true;

        var tempFile = Path.GetTempFileName();
        var tempBatFile = Path.ChangeExtension(tempFile, "ebuild-hooks.bat");
        File.Move(tempFile, tempBatFile);
        await File.WriteAllTextAsync(tempBatFile, batContent, cancellationToken);

        context.UpdateStatus?.Invoke(TargetStatus.Doing, $"正在准备执行编译周期脚本: {tempBatFile.EscapeMarkup()}");

        var process = new Process();
        context.EnvironmentVariables.LoadToStringDictionary(process.StartInfo.EnvironmentVariables);
        process.StartInfo.FileName = tempBatFile;

        process.StartInfo.StandardOutputEncoding =
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        process.StartInfo.RedirectStandardOutput = process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;

        void Handler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
                context.UpdateStatus?.Invoke(TargetStatus.Doing, e.Data);
        }

        process.OutputDataReceived += Handler;
        process.ErrorDataReceived += Handler;

        cancellationToken.Register(() =>
        {
            process.CancelErrorRead();
            process.CancelOutputRead();
            process.Kill();
        });
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync(cancellationToken);

        // 思考了一下还是决定不允许在构建脚本中接收输入（标准输入），尽管之前golang版本中已实现

        context.UpdateStatus?.Invoke(TargetStatus.Doing, $"脚本退出代码：{process.ExitCode}");

        File.Delete(tempBatFile);

        return process.ExitCode == 0;
    }
}