using EBuild.Config.Resolved;
using EBuild.Project;
using EBuild.Sources;
using EBuild.Toolchain;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using YamlDotNet.Serialization;

namespace EBuild.Commands.Base;

public class ProjectCommand : CommandBase
{
    [Option("-p|--project", Description = "工程根目录，默认为当前工作路径。")]
    public string ProjectRoot { get; set; } = Directory.GetCurrentDirectory();

    private readonly IDeserializer _deserializer;

    protected ResolvedConfig _resolvedConfig;

    public ProjectCommand(IDeserializer deserializer)
    {
        _deserializer = deserializer;
    }

    protected virtual bool ShowLoadConfig()
    {
        return true;
    }

    protected virtual IEnumerable<IToolchain> NeededToolchains { get; } = new IToolchain[0];

    protected sealed override int OnExecute(CommandLineApplication application)
    {
        try
        {
            ResolveProjectRoot();
            if (!CheckToolchians())
                return 1;
            if (ShowLoadConfig())
            {
                LoadProject();
                AnsiConsole.MarkupLine("[green]已启用配置：{0}[/]", _resolvedConfig.ConfigFile);
            }
        }
        catch (FileNotFoundException e)
        {
            Console.Error.WriteLine("找不到对应的配置文件，请检查！");
            return 1;
        }
        catch (UnauthorizedAccessException e)
        {
            Console.Error.WriteLine("没有权限访问配置文件。");
            return 1;
        }
        catch (DirectoryNotFoundException e)
        {
            Console.Error.WriteLine("找不到对应目录。");
            return 1;
        }

        return OnExecuteInternal(application);
    }

    private bool CheckToolchians()
    {
        foreach (var toolchain in NeededToolchains)
        {
            toolchain.Search(ProjectRoot);
            if (!toolchain.Exists())
            {
                AnsiConsole.MarkupLine("[red]找不到 {0}[/]",Markup.Escape(toolchain.Description));
                return false;
            }
        }
        return true;
    }

    private void ResolveProjectRoot()
    {
        ProjectRoot = Path.GetFullPath(ProjectRoot);
    }

    protected virtual int OnExecuteInternal(CommandLineApplication application)
    {
        return 0;
    }

    private void LoadProject()
    {
        var pwdResolver = PasswordFileResolver.FromProjectRootDir(ProjectRoot);
        _resolvedConfig = ResolvedConfig.Load(ProjectRoot, _deserializer, pwdResolver);
        _resolvedConfig.OutputDir = ProjectPath.GetDefaultOutputPath(ProjectRoot);
    }
}