using EBuild.Commands.Base;
using EBuild.Config.Resolved;
using EBuild.Project;

namespace EBuild.Plugins;

public class PluginContext
{
    public delegate void UpdateStatusDelegate(TargetStatus status, string log);

    public ResolvedTarget? BuildTarget { get; internal init; } = null;
    public ResolvedConfig ProjectConfig { get; }
    public EnvironmentVariables EnvironmentVariables { get; }
    public CancellationToken CancellationToken { get; internal init; }
    public UpdateStatusDelegate? UpdateStatus { get; internal init; }

    public PluginContext(ResolvedConfig projectConfig, EnvironmentVariables environmentVariables)
    {
        ProjectConfig = projectConfig;
        EnvironmentVariables = environmentVariables;
    }
}