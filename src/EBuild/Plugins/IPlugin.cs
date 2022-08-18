using EBuild.Config.Resolved;
using EBuild.Hooks;

namespace EBuild.Plugins;

public interface IPlugin
{
    /// <summary>
    /// 当触发指定<see cref="Hook">构建时期</see>时，会调用该函数。
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="hook"></param>
    /// <returns></returns>
    Task<bool> OnHook(PluginContext context, CancellationToken cancellationToken, Hook hook);
}