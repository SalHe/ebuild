using EBuild.Yaml.Converters;

namespace EBuild.Config;

public class E2Txt
{
    public enum NameStyleEnum
    {
        [EnumAlias("英文")] English,
        [EnumAlias("中文")] Chinese
    }

    public NameStyleEnum NameStyle { get; set; }
    public bool GenerateE { get; set; }
}