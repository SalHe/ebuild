﻿using System.ComponentModel;
using EBuild.Commands.Base;
using EBuild.Toolchain;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Description("检查工具链。")]
public class Toolchain : ProjectCommand<ProjectSettings>
{
    private readonly IEnumerable<IToolchain> _toolchains;

    public Toolchain(IEnumerable<IToolchain> toolchains, IDeserializer deserializer) : base(deserializer)
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