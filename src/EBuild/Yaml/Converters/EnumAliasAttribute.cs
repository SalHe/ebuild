using System.Reflection;

namespace EBuild.Yaml.Converters;

[AttributeUsage(AttributeTargets.Field)]
public class EnumAliasAttribute : Attribute
{
    public EnumAliasAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public static EnumAliasAttribute? GetEnumAliasAttribute(object? enumValue)
    {
        if (enumValue == null) return null;
        var type = enumValue.GetType();
        if (type is not { IsEnum: true }) return null;
        var enumName = type.GetEnumName(enumValue)!;
        return type.GetField(enumName)?.GetCustomAttribute<EnumAliasAttribute>();
    }
}