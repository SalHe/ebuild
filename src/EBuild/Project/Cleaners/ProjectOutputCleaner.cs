using EBuild.Config.Resolved;

namespace EBuild.Project.Cleaners;

public class ProjectOutputCleaner : ProjectCleaner
{
    public override string CleanContent => "编译结果";
    public override bool Once => true; // 与最原始的 golang 实现版的行为保持一致

    public override void Clean(ResolvedTarget target, ResolvedConfig projectConfig)
    {
        Console.WriteLine($"正在删除 {projectConfig.OutputDir}", true);
        Directory.Delete(projectConfig.OutputDir, true);
    }
}