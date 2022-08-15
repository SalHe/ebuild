using YamlDotNet.Serialization;

namespace EBuild.Config;

public class RootConfig
{
    public Project Project { get; set; }
    public IDictionary<string, string>? Scripts { get; set; }
    public IList<string> Excludes { get; set; } = new List<string>();
    public IList<string> Includes { get; set; } = new List<string>();
    public IList<string> ExcludeBuilds { get; set; } = new List<string>();

    [YamlMember(Alias = "e2txt")] public E2Txt E2Txt { get; set; }

    public Build Build { get; set; } = new Build()
    {
        Compiler = Compiler.Normal
    };

    public IList<Target>? Targets { get; set; }
}