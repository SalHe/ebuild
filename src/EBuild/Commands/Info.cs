namespace EBuild.Commands;

public static class Info
{
    public static void ShowProjectInfo(Config resolvedConfig)
    {
        Console.WriteLine("项目：{0}", resolvedConfig.RootConfig.Project.Name);
        Console.WriteLine("描述：{0}", resolvedConfig.RootConfig.Project.Description);
        Console.WriteLine("版本：{0}", resolvedConfig.RootConfig.Project.Version);
        Console.WriteLine("作者：{0}", resolvedConfig.RootConfig.Project.Author);
        Console.WriteLine("仓库：{0}", resolvedConfig.RootConfig.Project.Repository);
        Console.WriteLine("主页：{0}", resolvedConfig.RootConfig.Project.Homepage);
        Console.WriteLine();
        
        // TODO 完善信息
    }
}