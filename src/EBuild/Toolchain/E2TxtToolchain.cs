using EBuild.Config;

namespace EBuild.Toolchain;

public class E2TxtToolchain : GeneralToolchain
{
    public E2TxtToolchain() : base("e2txt", "http://e2ee.jimstone.com.cn/downloads/", "e2txt")
    {
    }

    public static IList<string> GeneralArgs(E2Txt e2txtConfig)
    {
        var args = new List<string>() { "-log", "-enc", "UTF-8" };

        switch (e2txtConfig.NameStyle)
        {
            case E2Txt.NameStyleEnum.English:
                args.Add("-ns");
                args.Add("1");
                break;
            case E2Txt.NameStyleEnum.Chinese:
                args.Add("-ns");
                args.Add("2");
                break;
        }

        if (e2txtConfig.GenerateE)
            args.Add("-e");

        return args;
    }

    private static IList<string> Args(string from, string to, string mode, E2Txt e2txtCofig)
    {
        var args = GeneralArgs(e2txtCofig);
        args.Add("-src");
        args.Add(from);
        args.Add("-dst");
        args.Add(to);
        args.Add("-mode");
        args.Add(mode);
        return args;
    }

    public static IList<string> E2TxtArgs(string source, string ecodeDir, E2Txt e2txtCofig)
    {
        return Args(source, ecodeDir, "e2txt", e2txtCofig);
    }

    public static IList<string> Txt2EArgs(string ecodeDir, string source, E2Txt e2txtCofig)
    {
        return Args(source, ecodeDir, "txt2e", e2txtCofig);
    }
}