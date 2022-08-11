using McMaster.Extensions.CommandLineUtils;

namespace EBuild.Commands.Base;

[HelpOption]
public class CommandBase
{
    protected virtual int OnExecute(CommandLineApplication application)
    {
        return 0;
    }
}