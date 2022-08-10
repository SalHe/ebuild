using EBuild.Config;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using YamlDotNet.Serialization;

namespace EBuild.Commands;

public class Config
{
    public string ProjectRootDir { get; private init; }
    public string ConfigFile => Path.GetFullPath("ebuild.yaml", ProjectRootDir);
    public RootConfig RootConfig { get; private set; }
    public IReadOnlyList<ResolvedTarget> ResolveTargets { get; private set; }

    private Config()
    {
    }

    public static Config Load(string projectRoot, IDeserializer deserializer)
    {
        var resolvedConfig = new Config
        {
            ProjectRootDir = projectRoot
        };
        resolvedConfig.RootConfig = deserializer.Deserialize<RootConfig>(File.OpenText(resolvedConfig.ConfigFile));
        var resolvedTargets = new List<ResolvedTarget>();
        resolvedConfig.ResolveTargets = resolvedTargets;

        // 自定义的源码
        var sourcesAdded = new HashSet<string>();
        foreach (var targetInConfig in resolvedConfig.RootConfig.Targets)
        {
            sourcesAdded.Add(Path.GetFullPath(targetInConfig.Source, projectRoot));
            resolvedTargets.Add(new ResolvedTarget(targetInConfig, TargetOrigin.Custom));
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

            var target = new Target()
            {
                Name = Path.GetFileNameWithoutExtension(file.Path),
                Source = sourcePath,
                Output = Path.ChangeExtension(Path.GetRelativePath(projectRoot, sourcePath),
                    null), // TODO 获取源码类型自动调整输出后缀
                Package = false,
                Build = resolvedConfig.RootConfig.Build
            };

            resolvedTargets.Add(new ResolvedTarget(target,
                ShouldBuild: !excludeBuildsMatcher.Match(file.Path).HasMatches));
        }

        return resolvedConfig;
    }
}

public enum TargetOrigin
{
    Search,
    Custom,
}

public record ResolvedTarget(Target Target, TargetOrigin Origin = TargetOrigin.Search, bool ShouldBuild = true);