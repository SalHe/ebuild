using EBuild.Global;
using EBuild.Project;
using EBuild.Sources;

namespace EBuild.Test;

public class ConfigTests
{
    private static string _projectDir = Path.GetFullPath("./test-project", Directory.GetCurrentDirectory());
    private static string _pwdFilePath = ProjectPath.GetSourcePasswordFilePath(_projectDir);
    private Commands.Config _resolvedConfig;

    [SetUp]
    public void ResolveConfig()
    {
        _resolvedConfig = Commands.Config.Load(_projectDir, Defaults.Deserializer,
            PasswordFileResolver.FromProjectRootDir(_projectDir));
    }

    [Test]
    public void ResolveTargetsTest()
    {
        Assert.That(_resolvedConfig.ResolveTargets.Count, Is.EqualTo(4));
    }

    [Test]
    public void ExcludeBuildsTest()
    {
        Assert.That(
            _resolvedConfig.ResolveTargets
                .Where(x => "not-build".Equals(x.Target.Name))
                .All(x => !x.ShouldBuild)
        );
    }

    [Test]
    public void PasswordResolveTest()
    {
        Assert.That(
            _resolvedConfig.ResolveTargets
                .Where(x => !string.IsNullOrEmpty(x.Password))
                .All(x => "12345".Equals(x.Password))
        );
    }
}