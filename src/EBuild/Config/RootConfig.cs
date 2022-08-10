using YamlDotNet.Serialization;

namespace EBuild.Config;

public class RootConfig
{
    public Project Project { get; set; }
    public IDictionary<string, string> Scripts { get; set; }
    public IList<string> Excludes { get; set; }
    public IList<string> Includes { get; set; }
    public IList<string> ExcludeBuilds { get; set; }

    [YamlMember(Alias = "e2txt")]
    public E2Txt E2Txt { get; set; }

    public Build Build { get; set; }
    public IList<Target> Targets { get; set; }
}