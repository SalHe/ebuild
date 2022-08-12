using EBuild.Config;
using EBuild.Toolchain;

namespace EBuild.Test;

internal class ToolchainTests
{
    private readonly E2TxtToolchain _e2txt;

    [Test]
    public void E2TxtArgsTest()
    {
        var source = "esrc!!!!!!!!!!!!!";
        var ecode = "ecode!!!!!!!!!!!!!";
        E2Txt config = new E2Txt { GenerateE = true, NameStyle = E2Txt.NameStyleEnum.Chinese };
        CollectionAssert.AreEquivalent(
            new List<string> { "-ns", "2", "-e", "-log", "-enc", "UTF-8" },
            E2TxtToolchain.GeneralArgs(config));

        var e2TxtArgs = E2TxtToolchain.E2TxtArgs(source, ecode, config);
        CollectionAssert.AreEquivalent(
            new List<string>
                { "-ns", "2", "-e", "-log", "-enc", "UTF-8", "-src", source, "-dst", ecode, "-mode", "e2t" },
            e2TxtArgs);
        Assert.That(e2TxtArgs[e2TxtArgs.IndexOf("-src") + 1], Is.EqualTo(source));
        Assert.That(e2TxtArgs[e2TxtArgs.IndexOf("-dst") + 1], Is.EqualTo(ecode));

        var txt2EArgs = E2TxtToolchain.Txt2EArgs(ecode, source, config);
        CollectionAssert.AreEquivalent(
            new List<string>
                { "-ns", "2", "-e", "-log", "-enc", "UTF-8", "-src", ecode, "-dst", source, "-mode", "t2e" },
            txt2EArgs);
        Assert.That(txt2EArgs[txt2EArgs.IndexOf("-src") + 1], Is.EqualTo(ecode));
        Assert.That(txt2EArgs[txt2EArgs.IndexOf("-dst") + 1], Is.EqualTo(source));
    }
}