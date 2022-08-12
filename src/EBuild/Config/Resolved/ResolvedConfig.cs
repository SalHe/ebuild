using EBuild.Project;
using EBuild.Sources;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using YamlDotNet.Serialization;

namespace EBuild.Config.Resolved;

public class ResolvedConfig
{
    public string ProjectRootDir { get; private init; }
    public string OutputDir { get; set; }
    public string ConfigFile => ProjectPath.GetConfigFilePath(ProjectRootDir);
    public RootConfig RootConfig { get; private set; }
    public IReadOnlyList<ResolvedTarget> ResolveTargets { get; private set; }

    private ResolvedConfig()
    {
    }

    public static ResolvedConfig Load(string projectRoot, IDeserializer deserializer,
        PasswordFileResolver passwordFileResolver)
    {
        var resolvedConfig = new ResolvedConfig
        {
            ProjectRootDir = projectRoot
        };
        resolvedConfig.RootConfig = deserializer.Deserialize<RootConfig>(File.OpenText(resolvedConfig.ConfigFile));
        var resolvedTargets = new List<ResolvedTarget>();
        resolvedConfig.ResolveTargets = resolvedTargets;

        // 自定义的源码
        var sourcesAdded = new HashSet<string>();
        if (resolvedConfig.RootConfig.Targets != null)
        {
            foreach (var targetInConfig in resolvedConfig.RootConfig.Targets)
            {
                targetInConfig.Source = Path.GetFullPath(targetInConfig.Source, projectRoot);
                if (targetInConfig.Build == null)
                    targetInConfig.Build = resolvedConfig.RootConfig.Build;

                sourcesAdded.Add(targetInConfig.Source);
                var pwd = passwordFileResolver.Resolve(targetInConfig.Source);
                resolvedTargets.Add(new ResolvedTarget(targetInConfig, TargetOrigin.Custom, Password: pwd));
            }
        }

        // 搜索的源码
        var matcher = new Matcher();
        matcher.AddIncludePatterns(resolvedConfig.RootConfig.Includes);
        matcher.AddExcludePatterns(resolvedConfig.RootConfig.Excludes);
        var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(projectRoot)));
        var excludeBuildsMatcher = new Matcher();
        excludeBuildsMatcher.AddIncludePatterns(resolvedConfig.RootConfig.ExcludeBuilds);
        foreach (var file in result.Files)
        {
            var sourcePath = Path.Join(projectRoot, file.Path);
            if (sourcesAdded.Contains(sourcePath))
                continue;
            var pwd = passwordFileResolver.Resolve(sourcePath);

            var target = new Target()
            {
                Name = string.Empty,
                Source = sourcePath,
                Output = Path.ChangeExtension(Path.GetRelativePath(projectRoot, sourcePath),
                    null), // TODO 获取源码类型自动调整输出后缀
                Package = false,
                Build = resolvedConfig.RootConfig.Build
            };

            resolvedTargets.Add(new ResolvedTarget(target, TargetOrigin.Search,
                !excludeBuildsMatcher.Match(file.Path).HasMatches, pwd));
        }

        return resolvedConfig;
    }
}

public enum TargetOrigin
{
    Search,
    Custom
}

public record ResolvedTarget(
    Target Target,
    TargetOrigin Origin = TargetOrigin.Search,
    bool ShouldBuild = true,
    string Password = "");