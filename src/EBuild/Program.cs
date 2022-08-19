using System.Reflection;
using System.Text;
using EBuild.Commands.SubCommands;
using EBuild.DependencyInjection;
using EBuild.Extensions;
using EBuild.Global;
using EBuild.Plugins;
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

services.AddImplementation<IPlugin, BuildHookScriptPlugin>();

var app = new CommandApp(new TypeRegistrar(services));
app.Configure(c =>
{
#if DEBUG
    c.PropagateExceptions();
    c.ValidateExamples();
#endif

    c.UseStrictParsing();
    c.SetApplicationVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString());

    c.AddCommand<InitCommand>("init")
        .WithExample(new[] { "init", "--default" })
        .WithExample(new[] { "init", "--project", "./examples/proj-1" });
    c.AddCommand<InfoCommand>("info")
        .WithExample(new[] { "info", "--project", "./examples/proj-1" });
    c.AddCommand<ToolchainCommand>("toolchain");
    c.AddCommand<E2TxtCommand>("e2txt")
        .WithExample(new[] { "e2txt", "./源码1.e", "我的DLL" });
    c.AddCommand<Txt2ECommand>("txt2e")
        .WithExample(new[] { "txt2e", "./源码1.e", "我的DLL" });
    c.AddCommand<BuildCommand>("build")
        .WithExample(new[] { "build", "./源码1.e", "我的DLL" });
    c.AddCommand<CleanCommand>("clean")
        .WithExample(new[] { "clean", "--ecode" })
        .WithExample(new[] { "clean", "--ecode", "--force" });
    c.AddCommand<RunCommand>("run");
});
return app.Run(args);