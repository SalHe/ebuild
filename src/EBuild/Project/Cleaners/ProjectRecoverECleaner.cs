using EBuild.Config.Resolved;

namespace EBuild.Project.Cleaners;

public class ProjectRecoverECleaner : ProjectCleaner
{
    public override string CleanContent => "*.recover.e(由文本格式代码恢复的源文件)";

    public override void Clean(ResolvedTarget target, ResolvedConfig projectConfig)
    {
        Console.WriteLine($"正在删除 {target.Target.GetRecoverEPath()}");
        File.Delete(target.Target.GetRecoverEPath());
    }
}