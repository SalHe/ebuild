using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EBuild.Yaml.Converters;

public class EnumConverter : IYamlTypeConverter
{
    public static readonly EnumConverter Instance = new EnumConverter();

    public bool Accepts(Type type)
    {
        return type.IsEnum;
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        var possibleNames = new HashSet<string>();
        var scalar = parser.Consume<Scalar>();
        possibleNames.Add(scalar.Value);
        possibleNames.Add(PascalCaseNamingConvention.Instance.Apply(scalar.Value));

        foreach (var value in Enum.GetValues(type))
        {
            var alias = type.GetField(Enum.GetName(type, value)!)!.GetCustomAttribute<EnumAliasAttribute>();
            if (alias != null && possibleNames.Contains(alias.Name))
            {
                return value;
            }
        }

        foreach (var possibleName in possibleNames)
        {
            if (Enum.TryParse(type, possibleName, out object? result))
                return result;
        }

        return null;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        var alias = type.GetCustomAttribute<EnumAliasAttribute>();
        emitter.Emit(alias != null ? new Scalar(alias.Name) : new Scalar(value?.ToString() ?? ""));
    }
}