using System.ComponentModel;
using EBuild.Config.Resolved;
using EBuild.Project;
using EBuild.Toolchain;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Description("将文本格式的代码转换为易语言源文件。")]
public class Txt2ECommand : E2TxtCommand
{
    public Txt2ECommand(IDeserializer deserializer, E2TxtToolchain e2txt) : base(deserializer, e2txt)
    {
    }

    protected override string GenerateHeading(WholeStatus status)
    {
        switch (status)
        {
            case WholeStatus.Doing:
                return "txt2e";
            case WholeStatus.Completed:
                return "[green]:check_mark:txt2e[/]";
            case WholeStatus.ErrorOccured:
                return "[red]:cross_mark:txt2e[/]";
        }

        return "";
    }

    protected override IList<string> GetArgs(ResolvedTarget target)
    {
        return E2TxtToolchain.Txt2EArgs(
            target.Target.GetECodeDir(), target.Target.GetRecoverEPath(), _resolvedConfig.RootConfig.E2Txt);
    }
}