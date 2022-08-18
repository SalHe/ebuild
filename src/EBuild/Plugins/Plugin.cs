using EBuild.Hooks;

namespace EBuild.Plugins;

public class Plugin : IPlugin
{
    public virtual Task<bool> OnHook(PluginContext context, CancellationToken cancellationToken, Hook hook)
    {
        return Task.FromResult(true);
    }
}