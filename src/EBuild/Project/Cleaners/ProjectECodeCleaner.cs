using EBuild.Config.Resolved;

namespace EBuild.Project.Cleaners;

public class ProjectECodeCleaner : OptionalProjectCleaner
{
    public override string CleanContent => "*.ecode(文本格式代码文件夹)";

    public override void Clean(ResolvedTarget target, ResolvedConfig projectConfig)
    {
        Console.WriteLine($"正在删除 {target.Target.GetECodeDir()}");
        Directory.Delete(target.Target.GetECodeDir(), true);
    }
}