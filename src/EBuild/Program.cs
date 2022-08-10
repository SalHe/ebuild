using EBuild.Config;
using EBuild.Yaml.Converters;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

var fs = File.Open(@"..\..\..\..\..\examples\simple\ebuild.yaml",
    FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
var rc = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .WithNamingConvention(HyphenatedNamingConvention.Instance)
    .WithTypeConverter(EnumConverter.Instance)
    .Build()
    .Deserialize<RootConfig>(new StreamReader(fs));

Console.WriteLine(
    new SerializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .Build()
        .Serialize(rc)
);