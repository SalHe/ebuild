using EBuild.Global;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

new HostBuilder()
    .ConfigureLogging((ctx, builder) => builder.AddConsole())
    .ConfigureServices((ctx, services) =>
    {
        services.AddLogging(builder => builder.ClearProviders());
        services.AddSingleton(_ => Defaults.Deserializer);
    })
    .RunCommandLineApplicationAsync<EBuild.Commands.EBuildCli>(args);