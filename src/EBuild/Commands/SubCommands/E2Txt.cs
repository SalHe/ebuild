using EBuild.Commands.Base;
using EBuild.Config.Resolved;
using EBuild.Project;
using EBuild.Toolchain;
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
                return "正在将易语言代码转换为文本格式代码...";
            case WholeStatus.Completed:
                return ":check_mark:将易语言代码转换为文本格式代码转换成功！";
            case WholeStatus.ErrorOccured:
                return ":cross_mark:将易语言代码转换为文本格式代码过程中出现错误，请注意查看！";
        }

        return "";
    }

    protected override Task<bool> OnDoTarget(ResolvedTarget target, UpdateTargetStatus updateTargetStatus)
    {
        updateTargetStatus(TargetStatus.Doing, "开始转换");

        // TODO 实现转换
        var args = E2TxtToolchain.E2TxtArgs(target.Target.Source, target.Target.GetECodeDir(),
            _resolvedConfig.RootConfig.E2Txt);
        updateTargetStatus(TargetStatus.Doing,
            string.Format("[grey]{0}[/]", _e2txt.ExecutablePath + " " + string.Join(" ", args)));

        return Task.FromResult(true);
    }

    protected override Task<bool> OnPreDoTarget(ResolvedTarget target, UpdateTargetStatus updateTargetStatus)
    {
        updateTargetStatus(TargetStatus.Waiting, "等待转换中...");
        return Task.FromResult(true);
    }
}