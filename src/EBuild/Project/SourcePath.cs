using EBuild.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EBuild.Project;
public static class SourcePath
{
    public static string GetECodeDir(string source)
    {
        return Path.ChangeExtension(source, "ecode");
    }

    public static string GetECodeDir(this Target target)
    {
        return GetECodeDir(target.Source);
    }
}
