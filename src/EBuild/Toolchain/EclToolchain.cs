using EBuild.Config;
using EBuild.Config.Resolved;

namespace EBuild.Toolchain;

public class EclToolchain : GeneralToolchain
{
    public EclToolchain() : base("易语言命令行编译工具", "https://bbs.125.la/forum.php?mod=viewthread&tid=14553929&highlight=ecl",
        "ecl")
    {
    }

    public static IList<string> Args(ResolvedTarget target, string outputDir)
    {
        var args = new List<string>() { "make", target.Target.Source, target.Target.OutputPath(outputDir) };
        if (target.Target.Package) args.Add("-p");
        else
        {
            args.AddRange(CompilerArgs(target.Target.Build?.Compiler, target.Target.CompileConfig,
                target.Target.CompileDescription)); // TODO 编译模块不用选择编译器
            if (!string.IsNullOrEmpty(target.Password))
                args.AddRange(new[] { "-pwd", target.Password });
        }

        return args;
    }

    public static IList<string> CompilerArgs(Compiler? compiler, string bmConfig, string bmDescription)
    {
        var args = new List<string>();
        if (compiler != null)
        {
            switch (compiler)
            {
                case Compiler.Static:
                    args.Add("-s");
                    break;
                case Compiler.Normal:
                    break;
                case Compiler.Independent:
                    args.Add("d");
                    break;
                case Compiler.BlackMoon:
                    args.Add("-bm");
                    break;
                case Compiler.BlackMoonAsm:
                    args.Add("-bm0");
                    break;
                case Compiler.BlackMoonCpp:
                    args.Add("-bm1");
                    break;
                case Compiler.BlackMoonMFC:
                    args.Add("-bm2");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compiler), compiler, null);
            }

            if (compiler.ToString()!.StartsWith("BlackMoon"))
            {
                if (!string.IsNullOrEmpty(bmConfig)) args.AddRange(new string[] { "-bmcfg", bmConfig });
                if (!string.IsNullOrEmpty(bmDescription)) args.AddRange(new string[] { "-bmdesc", bmDescription });
            }
        }

        return args;
    }
}