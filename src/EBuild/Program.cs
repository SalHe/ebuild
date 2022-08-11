using EBuild.Global;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

return await new HostBuilder()
    .ConfigureLogging((ctx, builder) => builder.AddConsole())
    .ConfigureServices((ctx, services) =>
    {
        services.AddLogging(builder => builder.ClearProviders());
        services.AddSingleton(_ => Defaults.Deserializer);
        services.AddSingleton(_ => Defaults.Serializer);
    })
    .RunCommandLineApplicationAsync<EBuild.Commands.EBuildCli>(args);