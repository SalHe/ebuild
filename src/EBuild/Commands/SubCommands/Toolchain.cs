using EBuild.Commands.Base;
using EBuild.Toolchain;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Command("toolchain")]
public class Toolchain : ProjectCommand
{
    public Toolchain(IEnumerable<IToolchain> toolchains, IDeserializer deserializer) : base(toolchains, deserializer)
    {
    }

    protected override bool ShowLoadConfig() => false;

    protected override int OnExecuteInternal(CommandLineApplication application)
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