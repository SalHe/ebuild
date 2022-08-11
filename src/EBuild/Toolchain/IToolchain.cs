namespace EBuild.Toolchain;

public interface IToolchain
{
    public string Description { get; }
    public string Link { get; }
    public string ExecutablePath { get; }
    public void Search(string projectRootDir);
    public bool Exists();
}