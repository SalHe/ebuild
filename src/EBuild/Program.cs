using System.Text;
using EBuild.Commands;
using EBuild.Extensions;
using EBuild.Global;
using EBuild.Project.Cleaners;
using EBuild.Toolchain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

try
{
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

            services.AddImplementation<ProjectCleaner, ProjectRecoverECleaner>();
            services.AddImplementation<ProjectCleaner, ProjectECodeCleaner>();
            services.AddImplementation<ProjectCleaner, ProjectOutputCleaner>();
        })
        .RunCommandLineApplicationAsync<EBuildCli>(args);
}
catch (OperationCanceledException e)
{
    AnsiConsole.MarkupLine("\n[red]操作已取消[/]");
    return 1;
}