using YamlDotNet.Serialization;

namespace EBuild.Sources;

public interface IPasswordResolver
{
    string Resolve(string source);
}

public class PasswordFileResolver : IPasswordResolver
{
    private Dictionary<string, string>? _pwdDict;
    private readonly string _pwdFilePath;
    private readonly string _projectRoot;

    public PasswordFileResolver(string passwordFilePath, string projectRoot)
    {
        _pwdFilePath = passwordFilePath;
        _projectRoot = projectRoot;
    }

    public string Resolve(string source)
    {
        if (_pwdDict == null)
        {
            var original = new Deserializer().Deserialize<Dictionary<string, string>>(File.OpenText(_pwdFilePath));
            _pwdDict = new Dictionary<string, string>();
            foreach (var (file, pwd) in original)
            {
                _pwdDict[Path.GetFullPath(file, _projectRoot)] = pwd;
            }
        }

        if (_pwdDict.ContainsKey(source))
            return _pwdDict[source];
        return string.Empty;
    }

    public static PasswordFileResolver FromProjectRootDir(string project)
    {
        return new PasswordFileResolver(Path.GetFullPath("./ebuild.pwd.yaml", project), project);
    }
}