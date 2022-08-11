namespace EBuild.Project;

public static class ProjectPath
{
    public static string GetConfigFilePath(string projectRootDir)
    {
        return Path.GetFullPath("ebuild.yaml", projectRootDir);
    }
    
    public static string GetSourcePasswordFilePath(string projectRootDir)
    {
        return Path.GetFullPath("ebuild.pwd.yaml", projectRootDir);
    }

    public static string GetDefaultOutputPath(string projectRootDir)
    {
        return Path.GetFullPath("ebuild-out", projectRootDir);
    }
}