using System.ComponentModel;
using EBuild.Commands.Base;
using EBuild.Plugins;
using EBuild.Toolchain;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Description("检查工具链。")]
public class ToolchainCommand : ProjectCommand<ProjectSettings>
{
    private readonly IEnumerable<IToolchain> _toolchains;

    public ToolchainCommand(IEnumerable<IToolchain> toolchains, IDeserializer deserializer,
        IEnumerable<IPlugin> plugins) : base(deserializer,plugins)
    {
        _toolchains = toolchains;
    }

    protected override bool ShowLoadConfig() => false;

    protected override int OnExecuteInternal()
    {
        var table = new Table();
        table.AddColumn("工具");
        table.AddColumn("安装路径");
        table.AddColumn("下载链接");

        var exitCode = 0;
        foreach (var toolchain in _toolchains)
        {
            toolchain.Search(ProjectRoot);
            table.AddRow(
                toolchain.Description,
                toolchain.Exists() ? toolchain.ExecutablePath : ":cross_mark:",
                toolchain.Link
            );
        }

        AnsiConsole.Write(table);
        return exitCode;
    }
}