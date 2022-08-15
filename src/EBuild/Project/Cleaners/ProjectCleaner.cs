using EBuild.Config.Resolved;

namespace EBuild.Project.Cleaners;

public abstract class ProjectCleaner : ICleaner<ResolvedTarget, ResolvedConfig>
{
    public virtual bool Optional => false;
    public virtual bool Once => false;
    public abstract string CleanContent { get; }
    public abstract void Clean(ResolvedTarget target, ResolvedConfig projectConfig);
}