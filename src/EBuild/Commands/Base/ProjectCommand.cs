using System.ComponentModel;
using EBuild.Config.Resolved;
using EBuild.Plugins;
using EBuild.Project;
using EBuild.Sources;
using EBuild.Toolchain;
using Spectre.Console;
using Spectre.Console.Cli;
using YamlDotNet.Serialization;

namespace EBuild.Commands.Base;

public class ProjectSettings : GeneralSettings
{
    [CommandOption("-p|--project")]
    [Description("工程根目录，默认为当前工作路径。")]
    [DefaultValue(".")]
    public string ProjectRoot { get; set; } = Directory.GetCurrentDirectory();
}

public class ProjectCommand<TSettings> : CommandBase<TSettings>
    where TSettings : ProjectSettings
{
    private readonly IDeserializer _deserializer;
    protected ResolvedConfig _resolvedConfig;
    protected string ProjectRoot { get; private set; } = "";

    public ProjectCommand(IDeserializer deserializer, IEnumerable<IPlugin> plugins) : base(plugins)
    {
        _deserializer = deserializer;
    }

    protected virtual bool ShowLoadConfig()
    {
        return true;
    }

    protected virtual IEnumerable<IToolchain> NeededToolchains { get; } = new IToolchain[0];

    public sealed override async Task<int> OnExecuteAsync(CancellationToken cancellationToken)
    {
        ProjectRoot = CommandSettings.ProjectRoot;
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

        return await OnExecuteInternalAsync(cancellationToken);
    }

    private bool CheckToolchians()
    {
        foreach (var toolchain in NeededToolchains)
        {
            toolchain.Search(ProjectRoot);
            if (!toolchain.Exists())
            {
                AnsiConsole.MarkupLine("[red]找不到 {0}[/]", Markup.Escape(toolchain.Description));
                return false;
            }
        }

        return true;
    }

    private void ResolveProjectRoot()
    {
        ProjectRoot = Path.GetFullPath(ProjectRoot);
    }

    protected virtual Task<int> OnExecuteInternalAsync(CancellationToken cancellationToken)
    {
        return Task.Run(OnExecuteInternal, cancellationToken);
    }

    protected virtual int OnExecuteInternal()
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