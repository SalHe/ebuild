using EBuild.Yaml.Converters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EBuild.Global;

public static class Defaults
{
    public static IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .WithTypeConverter(EnumConverter.Instance)
        .Build();
}