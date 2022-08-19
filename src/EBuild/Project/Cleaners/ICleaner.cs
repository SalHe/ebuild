namespace EBuild.Project.Cleaners;

public interface ICleaner<in TTarget, in TProject>
{
    bool Optional { get; }
    bool Once { get; }
    string CleanContent { get; }
    void Clean(TTarget target, TProject projectConfig);
}