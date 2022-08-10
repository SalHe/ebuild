using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using EBuild.Global;
using EBuild.Yaml.Converters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EBuild.Commands;

public class Entry
{
    public readonly Parser Cli;

    private readonly IDeserializer _deserializer;

    private Config _resolvedConfig;
    private Option<FileInfo> _projectOption;

    public Entry()
    {
        Cli = new CommandLineBuilder(InitCommand())
            .UseDefaults()
            .UseLocalizationResources(LocalizationResources.Instance) // TODO 汉化命令行提示
            .AddMiddleware(async (ctx, next) =>
            {
                if (ctx.ParseResult.HasOption(_projectOption))
                    LoadProject(ctx.ParseResult.GetValueForOption(_projectOption));

                next(ctx);
            })
            .Build();

        _deserializer = Defaults.Deserializer;
    }

    private RootCommand InitCommand()
    {
        _projectOption = new Option<FileInfo>(
            new[] { "--project", "-p" },
            () => new FileInfo(Directory.GetCurrentDirectory()),
            "工程所在目录。");

        var rootCommand = new RootCommand("ebuild 是一个构建于 e2txt, ecl 之上的、专注于易语言自动化构建的工具。");

        var infoCommand = new Command("info", "查看当前工程信息。")
        {
            _projectOption
        };
        infoCommand.SetHandler(() => Info.ShowProjectInfo(_resolvedConfig));

        rootCommand.AddCommand(infoCommand);
        return rootCommand;
    }

    private void LoadProject(FileInfo project)
    {
        try
        {
            _resolvedConfig = Config.Load(project.FullName, _deserializer);
        }
        catch (IOException e)
        {
            Console.Error.WriteLine("读取配置出错，请检查您是否指定了正确的ebuild工程目录。");
            throw;
        }
    }
}