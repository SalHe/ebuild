namespace EBuild.Yaml.Converters;

[AttributeUsage(AttributeTargets.Field)]
public class EnumAliasAttribute : Attribute
{
    public EnumAliasAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}