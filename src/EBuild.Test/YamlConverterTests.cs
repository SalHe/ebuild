using EBuild.Config;
using EBuild.Yaml.Converters;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace EBuild.Test;

public class YamlConverterTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestEnumConverter()
    {
        var doc = @"
- BlackMoon
- black-moon
- 黑月编译
";
        var compilers = new DeserializerBuilder()
            .WithTypeConverter(EnumConverter.Instance)
            .Build()
            .Deserialize<Compiler[]>(new Parser(new StringReader(doc)));
        foreach (var compiler in compilers) Assert.That(compiler, Is.EqualTo(Compiler.BlackMoon));
    }
}