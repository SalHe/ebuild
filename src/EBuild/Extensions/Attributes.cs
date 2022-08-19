using System.Reflection;

namespace EBuild.Extensions;

public static class Attributes
{
    public static bool GetEnumValueAttribute<TAttribute, TEnum>(TEnum enumValue, out TAttribute? attribute)
        where TEnum : Enum
        where TAttribute : Attribute
    {
        var type = enumValue.GetType();
        var enumName = type.GetEnumName(enumValue)!;
        attribute = type.GetField(enumName)?.GetCustomAttribute<TAttribute>();
        return attribute != null;
    }
}