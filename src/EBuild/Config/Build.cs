using EBuild.Yaml.Converters;

namespace EBuild.Config;

public enum Compiler
{
    [EnumAlias("静态编译")] Static,
    [EnumAlias("普通编译")] Normal,
    [EnumAlias("独立编译")] Independent,
    [EnumAlias("黑月编译")] BlackMoon,
    [EnumAlias("黑月汇编")] BlackMoonAsm,
    [EnumAlias("黑月C++")] BlackMoonCpp,
    [EnumAlias("黑月MFC")] BlackMoonMFC,
}

public class Build
{
    public Compiler Compiler { get; set; }
}