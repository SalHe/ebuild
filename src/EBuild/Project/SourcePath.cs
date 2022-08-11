using EBuild.Config;

namespace EBuild.Project;

public static class SourcePath
{
    public static string GetECodeDir(string source)
    {
        return Path.ChangeExtension(source, "ecode");
    }

    public static string GetECodeDir(this Target target)
    {
        return GetECodeDir(target.Source);
    }
}