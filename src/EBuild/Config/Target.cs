﻿using EBuild.Hooks;

namespace EBuild.Config;

public class Target
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }
    public string Output { get; set; }
    public bool Package { get; set; }
    public IDictionary<Hook, string> Hooks { get; set; }
    public Build? Build { get; set; }
    public string CompileConfig { get; set; }
    public string CompileDescription { get; set; }

    public string DisplayName(string projectRootDir)
    {
        return string.IsNullOrEmpty(Name)
            ? Path.GetRelativePath(projectRootDir, Source)
            : Name;
    }

    public string OutputPath(string outputDir)
    {
        return Path.GetFullPath(Output, outputDir);
    }
}