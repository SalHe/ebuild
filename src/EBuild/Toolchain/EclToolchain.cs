using System.Text.RegularExpressions;
using EBuild.Config;
using EBuild.Config.Resolved;
using EBuild.Sources;
using EBuild.Yaml.Converters;

namespace EBuild.Toolchain;

public class EclToolchain : GeneralToolchain
{
    private enum Error
    {
        [EnumAlias("编译成功")] Ok = 0,
        [EnumAlias("处理成功")] Success = 1,

        [EnumAlias("未定义类型的错误")] Unknown = -1,
        [EnumAlias("命令行有错误")] Param = -2,
        [EnumAlias("找不到文件")] FileNotFound = -3,
        [EnumAlias("文件无效")] FileInvalid = -4,
        [EnumAlias("编译失败")] Compile = -5,
        [EnumAlias("不支持的编译类型")] InvalidCompileType = -6,
        [EnumAlias("无法识别或无法运行的易语言程序")] ECannotStart = -7,
        [EnumAlias("无法获取易语言菜单")] CanNotGetMenu = -8,
        [EnumAlias("易语言意外结束")] Shutdown = -9,
        [EnumAlias("静态编译失败")] Static = -10,
        [EnumAlias("生成link.ini文件过程中出错")] MakeLinkIni = -11,
        [EnumAlias("老版黑月的相关数据无法定位")] BmInfo = -12,
        [EnumAlias("黑月编译失败")] BmCompile = -13,
        [EnumAlias("源码密码不正确")] Password = -14,
        [EnumAlias("缺乏易模块")] EC = -15,
        [EnumAlias("缺少支持库")] ELib = -16,
        [EnumAlias("启动易语言超时")] StartTimeout = -17,
        [EnumAlias("编译超时")] CompileTimeout = -18,
        [EnumAlias("不支持易包编译")] NotSupportEPkg = -19,
    }

    public override IList<EnvironmentVariable> EnvironmentVariables => new List<EnvironmentVariable>()
    {
        new("ECL_DIR", "ecl安装路径", () => Path.GetDirectoryName(ExecutablePath) ?? ""),
    };

    public EclToolchain() : base("易语言命令行编译工具", "https://bbs.125.la/forum.php?mod=viewthread&tid=14553929&highlight=ecl",
        "ecl")
    {
    }

    public static IList<string> Args(ResolvedTarget target, string outputDir, bool hideSecret = false)
    {
        return Args(
            target.Target.Source,
            target.Target.OutputPath(outputDir),
            target.SourceMeta,
            target.Target.Build?.Compiler,
            target.Target.CompileConfig,
            target.Target.CompileDescription,
            target.Password,
            target.Target.Package,
            hideSecret
        );
    }

    public static IList<string> Args(string source, string outputPath, ESourceMeta? sourceMeta, Compiler? compiler,
        string compileConfig, string compileDescription, string password, bool isPackage,
        bool hideSecret = false)
    {
        var args = new List<string>() { "make", source, outputPath };
        if (isPackage) args.Add("-p");
        else
        {
            if (sourceMeta?.TargetType != TargetType.LinuxECom &&
                sourceMeta?.TargetType != TargetType.WinECom) // 编译模块不用选择编译器
                args.AddRange(CompilerArgs(compiler, compileConfig,
                    compileDescription));
            if (!string.IsNullOrEmpty(password))
                args.AddRange(new[] { "-pwd", hideSecret ? "******" : password });
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
                    args.Add("-d");
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

    private static readonly Regex errorMatcher = new Regex(@"\(错误:(-\d+)\)(.+)");

    public static bool TryMatchError(string log, out bool compileOk, out int errorCode, out string tip)
    {
        var match = errorMatcher.Match(log);
        if (!match.Success)
        {
            errorCode = 0;
            tip = "";
            compileOk = false;
            return false;
        }

        errorCode = int.Parse(match.Groups[1].Value);
        var error = (Error)errorCode;
        // tip = EnumAliasAttribute.GetEnumAliasAttribute(error)?.Name ?? "ebuild暂未记录该错误代码";
        tip = match.Groups[2].Value;
        compileOk = error is Error.Ok or Error.Success;
        return true;
    }
}