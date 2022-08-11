using McMaster.Extensions.CommandLineUtils;
using YamlDotNet.Serialization;

namespace EBuild.Commands;

[Command("info", Description = "查看当前工程信息。")]
public class Info : ProjectCommand
{
    public Info(IDeserializer deserializer) : base(deserializer)
    {
    }

    protected override int OnExecuteInternal(CommandLineApplication application)
    {
        ShowProjectInfo(_resolvedConfig);
        return 0;
    }

    public void ShowProjectInfo(Config resolvedConfig)
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