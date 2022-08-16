using System.Reflection;
using System.Text;
using EBuild.Commands.SubCommands;
using EBuild.DependencyInjection;
using EBuild.Extensions;
using EBuild.Global;
using EBuild.Project;
using EBuild.Project.Cleaners;
using EBuild.Toolchain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
services.AddLogging(builder => builder.ClearProviders());

services.AddSingleton(_ => Defaults.Deserializer);
services.AddSingleton(_ => Defaults.Serializer);

services.AddSingleton(
    x => new EnvironmentVariables(x.GetService<IEnumerable<IToolchain>>()!)
    {
        new("EBUILD_EXECUTABLE_PATH", "ebuild可执行文件路径", () => Assembly.GetExecutingAssembly().Location ?? "")
    });

services.AddImplementation<IToolchain, EclToolchain>();
services.AddImplementation<IToolchain, E2TxtToolchain>();
services.AddImplementation<IToolchain, ELangToolchain>();

services.AddImplementation<ProjectCleaner, ProjectRecoverECleaner>();
services.AddImplementation<ProjectCleaner, ProjectECodeCleaner>();
services.AddImplementation<ProjectCleaner, ProjectOutputCleaner>();

var app = new CommandApp(new TypeRegistrar(services));
app.Configure(c =>
{
#if DEBUG
    c.PropagateExceptions();
#endif
    
    c.UseStrictParsing();

    c.AddCommand<Init>("init");
    c.AddCommand<Info>("info");
    c.AddCommand<Toolchain>("toolchain");
    c.AddCommand<E2Txt>("e2txt");
    c.AddCommand<Txt2E>("txt2e");
    c.AddCommand<BuildCommand>("build");
    c.AddCommand<CleanCommand>("clean");
    c.AddCommand<RunCommand>("run");
});
return app.Run(args);