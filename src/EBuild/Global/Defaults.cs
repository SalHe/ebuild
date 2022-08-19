using EBuild.Yaml.Converters;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;

namespace EBuild.Global;

internal class LiteralMultilineEventEmitter : ChainedEventEmitter
{
    public LiteralMultilineEventEmitter(IEventEmitter nextEmitter) : base(nextEmitter)
    {
    }

    public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
    {
        if (eventInfo.Source.Type == typeof(string) && eventInfo.Source.Value is string value && value.Contains("\n"))
            eventInfo.Style = ScalarStyle.Literal;

        base.Emit(eventInfo, emitter);
    }
}

public static class Defaults
{
    public static IDeserializer Deserializer = new DeserializerBuilder()
        .WithDefaults()
        .Build();

    public static ISerializer Serializer = new SerializerBuilder()
        .WithDefaults()
        .WithEventEmitter(e => new LiteralMultilineEventEmitter(e))
        .Build();

    private static T WithDefaults<T>(this T builder) where T : BuilderSkeleton<T>
    {
        return builder
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .WithTypeConverter(EnumConverter.Instance);
    }
}