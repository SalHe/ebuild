using System.Collections.Specialized;
using EBuild.Toolchain;

namespace EBuild.Project;

public class EnvironmentVariables : List<EnvironmentVariable>
{
    private readonly IEnumerable<IToolchain> _toolchains;

    public EnvironmentVariables(IEnumerable<IToolchain> toolchains)
    {
        _toolchains = toolchains;
    }

    public void ForProject(string projectRoot, string outputDir)
    {
        foreach (var toolchain in _toolchains)
        {
            toolchain.Search(projectRoot);
            AddRange(toolchain.EnvironmentVariables);
        }

        Add(new("EBUILD_PROJECT_ROOT_DIR", "工程根目录", () => projectRoot));
        Add(new("EBUILD_PROJECT_OUTPUT_DIR", "构建输出目录", () => outputDir));
    }

    public void LoadToStringDictionary(StringDictionary d)
    {
        foreach (var variable in this)
        {
            d[variable.VariableName] = variable.Value();
        }
    }
}