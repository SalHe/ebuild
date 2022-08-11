using EBuild.Yaml.Converters;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands;

[Command("info", Description = "查看当前工程信息。")]
public class Info : ProjectCommand
{
    [Option("--abs", Description = "显示源码的绝对路径")]
    public bool AbsolutePath { get; set; } = false;

    [Option("--password", Description = "显示源码的密码")]
    public bool ShowPassword { get; set; }

    public Info(IDeserializer deserializer) : base(deserializer)
    {
    }

    protected override int OnExecuteInternal(CommandLineApplication application)
    {
        ShowProjectIntro();
        ShowScripts();
        ShowE2txt();
        ShowResolvedTargets();
        return 0;
    }

    private void ShowProjectIntro()
    {
        AnsiConsole.MarkupLine("[green bold]当前项目信息[/]");
        Console.WriteLine("项目：{0}", _resolvedConfig.RootConfig.Project.Name);
        Console.WriteLine("描述：{0}", _resolvedConfig.RootConfig.Project.Description);
        Console.WriteLine("版本：{0}", _resolvedConfig.RootConfig.Project.Version);
        Console.WriteLine("作者：{0}", _resolvedConfig.RootConfig.Project.Author);
        Console.WriteLine("仓库：{0}", _resolvedConfig.RootConfig.Project.Repository);
        Console.WriteLine("主页：{0}", _resolvedConfig.RootConfig.Project.Homepage);
        Console.WriteLine();
    }

    private void ShowScripts()
    {
        AnsiConsole.MarkupLine("[green bold]脚本[/]");
        foreach (var (key, value) in _resolvedConfig.RootConfig.Scripts)
            Console.WriteLine(key);
        Console.WriteLine();
    }

    private void ShowE2txt()
    {
        AnsiConsole.MarkupLine("[green bold]e2txt配置[/]");
        Console.WriteLine("风格：{0}",
            EnumAliasAttribute.GetEnumAliasAttribute(_resolvedConfig.RootConfig.E2Txt.NameStyle)!.Name);
        Console.WriteLine("生成易代码：{0}", _resolvedConfig.RootConfig.E2Txt.GenerateE);
        Console.WriteLine();
    }

    private void ShowResolvedTargets()
    {
        AnsiConsole.MarkupLine("[green bold]构建目标 [grey]{0}[/][/]", _resolvedConfig.OutputDir);
        var table = new Table();
        table.AddColumn("源码");
        table.AddColumn("来源");
        if (ShowPassword)
            table.AddColumn("密码");
        table.AddColumn("构建");
        table.AddColumn("编译器");
        table.AddColumn("输出");
        foreach (var target in _resolvedConfig.ResolveTargets)
        {
            var cols = new List<string>();
            cols.Add(AbsolutePath
                ? target.Target.Source
                : Path.GetRelativePath(ProjectRoot, target.Target.Source));
            switch (target.Origin)
            {
                case TargetOrigin.Custom:
                    cols.Add("自定义");
                    break;
                case TargetOrigin.Search:
                    cols.Add("搜索");
                    break;
            }

            if (ShowPassword)
                cols.Add(target.Password);

            cols.Add(target.ShouldBuild ? "[green]:check_mark:[/]" : "[red]:cross_mark:[/]");
            cols.Add(EnumAliasAttribute.GetEnumAliasAttribute(target.Target.Build.Compiler)?.Name ??
                     target.Target.Build.Compiler.ToString());
            cols.Add(target.Target.Output);

            table.AddRow(cols.ToArray());
        }

        AnsiConsole.Write(table);
    }
}