using EBuild.Config.Resolved;
using EBuild.Project;
using EBuild.Toolchain;
using McMaster.Extensions.CommandLineUtils;
using YamlDotNet.Serialization;

namespace EBuild.Commands.SubCommands;

[Command(
    "txt2e",
    Description = "将文本格式的代码转换为易语言源文件。",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.CollectAndContinue
)]
public class Txt2E : E2Txt
{
    public Txt2E(IDeserializer deserializer, E2TxtToolchain e2txt) : base(deserializer, e2txt)
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