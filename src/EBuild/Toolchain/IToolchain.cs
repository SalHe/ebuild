namespace EBuild.Toolchain;

public record EnvironmentVariable(string VariableName, string Description, Func<string> Value);

public interface IToolchain
{
    public string Description { get; }
    public string Link { get; }
    public string ExecutablePath { get; }
    public IList<EnvironmentVariable> EnvironmentVariables { get; }
    public void Search(string projectRootDir);
    public bool Exists();
}