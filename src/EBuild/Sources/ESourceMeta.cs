using EBuild.Yaml.Converters;

namespace EBuild.Sources;

public enum SourceType
{
    [EnumAlias("源码")] Src = 1,
    [EnumAlias("模块")] ECom = 3,
}

public enum TargetType
{
    [ESourceTargetType(Extension = "exe")] WinForm = 0,
    [ESourceTargetType(Extension = "exe")] WinConsole = 1,
    [ESourceTargetType(Extension = "dll")] WinDll = 2,
    [ESourceTargetType(Extension = "ec")] WinEc = 1000,
    [ESourceTargetType(Extension = null)] LinuxConsole = 10000,
    [ESourceTargetType(Extension = null)] LinuxEc = 11000
}

public class ESourceMeta
{
    public SourceType SourceType { get; set; }
    public TargetType TargetType { get; set; }

    private ESourceMeta()
    {
    }

    public static ESourceMeta? FromSource(string sourcePath)
    {
        try
        {
            using var fs = File.OpenRead(sourcePath);
            using var br = new BinaryReader(fs);
            fs.Seek(124L, SeekOrigin.Begin);
            var st = (SourceType)br.ReadInt32();
            fs.Seek(132L, SeekOrigin.Begin);
            var tt = (TargetType)br.ReadInt32();
            return new ESourceMeta()
            {
                SourceType = st,
                TargetType = tt,
            };
        }
        catch (Exception e)
        {
            return null;
        }
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class ESourceTargetTypeAttribute : Attribute
{
    public string? Extension { get; set; }
}