using System.Text;
using EBuild.Commands;
using EBuild.Extensions;
using EBuild.Global;
using EBuild.Toolchain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

return await new HostBuilder()
    .ConfigureLogging((ctx, builder) => builder.AddConsole())
    .ConfigureServices((ctx, services) =>
    {
        services.AddLogging(builder => builder.ClearProviders());

        services.AddSingleton(_ => Defaults.Deserializer);
        services.AddSingleton(_ => Defaults.Serializer);

        services.AddImplementation<IToolchain, EclToolchain>();
        services.AddImplementation<IToolchain, E2TxtToolchain>();
        services.AddImplementation<IToolchain, ELangToolchain>();
    })
    .RunCommandLineApplicationAsync<EBuildCli>(args);