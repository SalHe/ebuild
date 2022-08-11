using System.Reflection;

namespace EBuild.Toolchain;

public class GeneralToolchain : IToolchain
{
    private readonly string _executableName;
    public string Description { get; }
    public string Link { get; }
    public string ExecutablePath { get; protected set; } = string.Empty;

    public GeneralToolchain(string description, string link, string executableName)
    {
        _executableName = executableName;
        Description = description;
        Link = link;
    }

    private string GetExecutablePath(string root, string name)
    {
        return Path.GetFullPath(Path.Join(root, ".toolchain", name, name + ".exe"));
    }

    public virtual void Search(string projectRootDir)
    {
        var possible = new List<string>()
        {
            GetExecutablePath(projectRootDir, _executableName),
            GetExecutablePath(Directory.GetCurrentDirectory(), _executableName),
            GetExecutablePath(Assembly.GetExecutingAssembly().Location, _executableName),
        };
        foreach (var path in possible)
        {
            if (File.Exists(path))
            {
                ExecutablePath = path;
                return;
            }
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
}