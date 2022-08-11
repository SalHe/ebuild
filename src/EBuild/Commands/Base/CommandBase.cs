﻿using System.Reflection;
using McMaster.Extensions.CommandLineUtils;

namespace EBuild.Commands.Base;

[HelpOption(Description = "查看帮助信息")]
[VersionOptionFromMember(Description = "查看当前版本", MemberName = nameof(VersionText))]
public class CommandBase
{
    protected virtual int OnExecute(CommandLineApplication application)
    {
        return 0;
    }

    public static string VersionText() => Assembly.GetExecutingAssembly().GetName().Version.ToString();
}