using EBuild.Global;

namespace EBuild.Test;

public class ConfigTests
{
    private string _projectDir = Path.GetFullPath("./test-project", Directory.GetCurrentDirectory());
    private Commands.Config _resolvedConfig;

    [SetUp]
    public void ResolveConfig()
    {
        _resolvedConfig = Commands.Config.Load(_projectDir, Defaults.Deserializer);
    }

    [Test]
    public void ResolveTargetsTest()
    {
        Assert.That(_resolvedConfig.ResolveTargets.Count, Is.EqualTo(3));
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
}