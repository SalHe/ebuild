using Microsoft.Win32;

namespace EBuild.Toolchain;

public class ELangToolchain : GeneralToolchain
{
    public override IList<EnvironmentVariable> EnvironmentVariables => new List<EnvironmentVariable>()
    {
        new("ELANG_DIR", "易语言安装路径", () => Path.GetDirectoryName(ExecutablePath) ?? ""),
    };

    public ELangToolchain() : base("易语言", "http://www.eyuyan.com/pdown.htm", "e")
    {
    }

    public override void Search(string projectRootDir)
    {
        base.Search(projectRootDir);
        if (!Exists())
        {
            using var registryKey = Registry.CurrentUser.OpenSubKey("Software\\FlySky\\E\\Install");
            var eLibDir = registryKey.GetValue("Path")?.ToString() ?? "";
            ExecutablePath = Path.Combine(Path.GetFullPath("..", eLibDir), "e.exe");
            if (!File.Exists(ExecutablePath)) ExecutablePath = "";
        }
    }
}