using EBuild.Config;
using EBuild.Project;
using EBuild.Toolchain;
using EBuild.Yaml.Converters;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands;

public class Init : ProjectCommand
{
    [Option("-d|--default", Description = "采用默认配置初始化工程。")]
    public bool DefaultInit { get; set; }

    private readonly ISerializer _serializer;

    public Init(IDeserializer deserializer, ISerializer serializer, IEnumerable<IToolchain> toolchains) : base(
        toolchains, deserializer)
    {
        _serializer = serializer;
    }

    protected override bool ShowLoadConfig() => false;

    protected override int OnExecuteInternal(CommandLineApplication application)
    {
        var configFilePath = ProjectPath.GetConfigFilePath(ProjectRoot);
        if (Directory.Exists(ProjectRoot) && Directory.GetFiles(ProjectRoot).Length > 0)
        {
            AnsiConsole.MarkupLine("[red]目录不为空，不能创建ebuild工程：{0}[/]", configFilePath);
            return 1;
        }

        var rootConfig = CreateRootConfig();
        Directory.CreateDirectory(ProjectRoot);
        try
        {
            File.WriteAllText(configFilePath, _serializer.Serialize(rootConfig));
            File.WriteAllText(Path.GetFullPath(".gitignore", ProjectRoot), @"
# 恢复出来的易语言源文件和密码文件不纳入版本控制
*.recover.e
ebuild.pwd.yaml
**/*.ecode/log

# 易语言产生的备份源码文件
*.bak

# 编译输出
ebuild-out/");
            File.WriteAllText(ProjectPath.GetSourcePasswordFilePath(ProjectRoot), "./带密码的源码.e: 123456");
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine("[red]创建工程失败！{0}[/]", e);
            return 1;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]创建工程完成。[/]");
        AnsiConsole.MarkupLine("[green]有关工程的配置可以参见ebuild示例代码：https://github.com/SalHe/ebuild/tree/master/examples[/]");
        AnsiConsole.MarkupLine("[green]此外，您也可以去ebuild官方网站查看更加详细的文档：https://salhe.github.io/ebuild/[/]");
        return 0;
    }

    private RootConfig CreateRootConfig()
    {
        var defaultProjectName = Path.GetFileName(ProjectRoot);
        var project = new EBuild.Config.Project()
        {
            Name = defaultProjectName,
            Version = "1.0",
            Description = $"{Environment.UserName}的工程：{defaultProjectName}",
            Author = Environment.UserName,
            Repository = $"https://github.com/{Environment.UserName}/{defaultProjectName}",
            Homepage = $"https://github.com/{Environment.UserName}"
        };
        var rootConfig = new RootConfig()
        {
            Project = project,
            Scripts = new Dictionary<string, string>()
            {
                {
                    "hello", "@echo off\necho Hello ebuild!"
                }
            },
            Includes = new List<string>()
            {
                "**/*.e"
            },
            Excludes = new List<string>()
            {
                "**/*.recover.e",
                "**/*.ecode/**.e",
                "**/*.代码/**.e"
            },
            ExcludeBuilds = new List<string>()
            {
                "./scripts/**/*.e"
            },
            Build = new Build()
            {
                Compiler = Compiler.Static
            },
            E2Txt = new E2Txt()
            {
                GenerateE = true,
                NameStyle = E2Txt.NameStyleEnum.Chinese
            }
        };
        if (!DefaultInit)
        {
            project.Name = AnsiConsole.Ask("工程", project.Name);
            project.Version = AnsiConsole.Ask("版本", project.Version);
            project.Description = AnsiConsole.Ask("说明", project.Description);
            project.Author = AnsiConsole.Ask("作者", project.Author);
            project.Repository = AnsiConsole.Ask("仓库", project.Repository);
            project.Homepage = AnsiConsole.Ask("主页", project.Homepage);

            rootConfig.E2Txt.GenerateE = AnsiConsole.Confirm(Markup.Escape("[e2txt]生成易代码"), rootConfig.E2Txt.GenerateE);
            rootConfig.E2Txt.NameStyle = AnsiConsole.Prompt(
                new SelectionPrompt<E2Txt.NameStyleEnum>()
                    .Title(Markup.Escape("[e2txt]命名风格"))
                    .UseConverter(DisplaySelector)
                    .AddChoices(Enum.GetValues<E2Txt.NameStyleEnum>())
            );
            AnsiConsole.MarkupLine(Markup.Escape("[e2txt]") + "命名风格：{0}", DisplaySelector(rootConfig.E2Txt.NameStyle));

            rootConfig.Build.Compiler = AnsiConsole.Prompt(
                new SelectionPrompt<Compiler>()
                    .Title("全局默认编译器")
                    .UseConverter(DisplaySelector)
                    .AddChoices(Enum.GetValues<Compiler>())
            );
            AnsiConsole.MarkupLine("全局默认编译器：{0}", DisplaySelector(rootConfig.Build.Compiler));
        }

        return rootConfig;
    }

    private static string DisplaySelector<T>(T enumValue)
    {
        return EnumAliasAttribute.GetEnumAliasAttribute(enumValue).Name;
    }
}