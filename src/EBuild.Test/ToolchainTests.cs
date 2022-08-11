using EBuild.Toolchain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBuild.Test;
internal class ToolchainTests
{

    private readonly E2TxtToolchain _e2txt;

    [Test]
    public void E2TxtArgsTest()
    {
        var source = "esrc!!!!!!!!!!!!!";
        var ecode = "ecode!!!!!!!!!!!!!";
        Config.E2Txt config = new Config.E2Txt() { GenerateE = true, NameStyle = Config.E2Txt.NameStyleEnum.Chinese };
        CollectionAssert.AreEquivalent(
            new List<string> { "-ns", "2", "-e", "-log", "-enc", "UTF-8" },
            E2TxtToolchain.GeneralArgs(config));
        CollectionAssert.AreEquivalent(
            new List<string> { "-ns", "2", "-e", "-log", "-enc", "UTF-8", "-src", source, "-dst", ecode, "-mode", "e2txt" },
            E2TxtToolchain.E2TxtArgs(source, ecode, config));
        CollectionAssert.AreEquivalent(
            new List<string> { "-ns", "2", "-e", "-log", "-enc", "UTF-8", "-src", ecode, "-dst", source, "-mode", "txt2e" },
            E2TxtToolchain.Txt2EArgs(ecode, source, config));
    }

}
