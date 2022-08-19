using System.Diagnostics;

namespace EBuild.Toolchain;

public abstract class GeneralToolchain : IToolchain
{
    private readonly string _executableName;

    public GeneralToolchain(string description, string link, string executableName)
    {
        _executableName = executableName;
        Description = description;
        Link = link;
    }

    public string Description { get; }
    public string Link { get; }
    public string ExecutablePath { get; protected set; } = string.Empty;
    public abstract IList<EnvironmentVariable> EnvironmentVariables { get; }

    public virtual void Search(string projectRootDir)
    {
        var possible = new List<string>()
        {
            GetExecutablePath(projectRootDir, _executableName),
            GetExecutablePath(Directory.GetCurrentDirectory(), _executableName),
            GetExecutablePath(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? "") ?? "",
                _executableName)
        };
        foreach (var path in possible)
            if (File.Exists(path))
            {
                ExecutablePath = path;
                return;
            }

        ExecutablePath = Environment.GetEnvironmentVariable("PATH")!
            .Split(";")
            .Where(s => File.Exists(Path.Combine(s, _executableName + ".exe")))
            .FirstOrDefault(string.Empty);
    }

    public bool Exists()
    {
        return !string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath);
    }

    private string GetExecutablePath(string root, string name)
    {
        return Path.GetFullPath(Path.Join(root, ".toolchain", name, name + ".exe"));
    }
}