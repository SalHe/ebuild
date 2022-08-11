using EBuild.Commands.Base;
using EBuild.Commands.SubCommands;
using McMaster.Extensions.CommandLineUtils;

namespace EBuild.Commands;

[Command("ebuild", Description = "ebuild 是一个构建于 e2txt, ecl 之上的、专注于易语言自动化构建的工具。")]
[Subcommand(
    typeof(Init),
    typeof(Info),
    typeof(SubCommands.Toolchain),
    typeof(E2Txt)
)]
public class EBuildCli : CommandBase
{
}