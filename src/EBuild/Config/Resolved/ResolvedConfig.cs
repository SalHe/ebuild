using EBuild.Extensions;
using EBuild.Project;
using EBuild.Sources;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Spectre.Console;
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
            var sourcePath = Path.GetFullPath(file.Path, projectRoot);
            if (sourcesAdded.Contains(sourcePath))
                continue;
            var pwd = passwordFileResolver.Resolve(sourcePath);

            var target = new Target()
            {
                Name = string.Empty,
                Source = sourcePath,
                Output = Path.ChangeExtension(Path.GetRelativePath(projectRoot, sourcePath),
                    null),
                Package = false,
                Build = resolvedConfig.RootConfig.Build
            };

            resolvedTargets.Add(new ResolvedTarget(target, TargetOrigin.Search,
                !excludeBuildsMatcher.Match(file.Path).HasMatches, pwd));
        }

        foreach (var target in resolvedConfig.ResolveTargets)
        {
            if (string.IsNullOrEmpty(target.Password) && !Path.HasExtension(target.Target.Output) &&
                target.SourceMeta != null)
            {
                if (target.Origin == TargetOrigin.Custom)
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]您可能未给{Markup.Escape("[")}{{0}}{Markup.Escape("]")}指定输出后缀，易语言会自动加上后缀，请注意。[/]",
                        Markup.Escape(target.Target.Name));
                }
                else
                {
                    Attributes.GetEnumValueAttribute(target.SourceMeta.TargetType,
                        out ESourceTargetTypeAttribute? attribute);
                    target.Target.Output = Path.ChangeExtension(target.Target.Output, attribute!.Extension);
                }
            }

            if (target.SourceMeta == null)
                AnsiConsole.MarkupLine("[yellow]{0} 可能不是一个合法的源文件。[/]", Markup.Escape(target.Target.Source));
        }

        return resolvedConfig;
    }
}

public enum TargetOrigin
{
    Search,
    Custom
}

public class ResolvedTarget
{
    public ResolvedTarget(Target Target,
        TargetOrigin Origin = TargetOrigin.Search,
        bool ShouldBuild = true,
        string Password = "")
    {
        this.Target = Target;
        this.Origin = Origin;
        this.ShouldBuild = ShouldBuild;
        this.Password = Password;
        this.SourceMeta = ESourceMeta.FromSource(Target.Source);
    }

    public Target Target { get; init; }
    public TargetOrigin Origin { get; init; }
    public bool ShouldBuild { get; init; }
    public string Password { get; init; }
    public ESourceMeta? SourceMeta { get; }
}