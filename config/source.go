package config

import (
	"strings"
)

type CompilerType string

const (
	CompilerBlackMoon    = "黑月"
	CompilerBlackMoonAsm = "黑月汇编"
	CompilerBlackMoonCPP = "黑月C++"
	CompilerBlackMoonMFC = "黑月MFC"
	CompilerStatic       = "静态编译"
	CompilerIndependent  = "独立编译"
)

type Source struct {
	Source             string       `mapstructure:"source"`
	Output             string       `mapstructure:"output"`
	Compiler           CompilerType `mapstructure:"compiler"`
	CompileConfig      string       `mapstructure:"compile-config"`
	CompileDescription string       `mapstructure:"compile-description"`
	Package            bool         `mapstructure:"package"`
}

func (c *CompilerType) Args() []string {
	switch *c {
	case CompilerBlackMoon:
		return []string{"-bm"}
	case CompilerBlackMoonAsm:
		return []string{"-bm0"}
	case CompilerBlackMoonCPP:
		return []string{"-bm1"}
	case CompilerBlackMoonMFC:
		return []string{"-bm2"}
	case CompilerStatic:
		return []string{"-s"}
	case CompilerIndependent:
		return []string{"-d"}
	}
	return []string{}
}

func (s *Source) CompileArgs(pwd string) (args []string) {
	args = append(args, s.Source, s.Output)

	if s.Package {
		args = append(args, "-p")
	} else {
		args = append(args, s.Compiler.Args()...)
		if strings.HasPrefix(string(s.Compiler), "-bm") {
			if len(s.CompileConfig) > 0 {
				args = append(args, "-bmcfg", s.CompileConfig)
			}
			if len(s.CompileDescription) > 0 {
				args = append(args, "-bmdesc", s.CompileDescription)
			}
		}

		if len(pwd) > 0 {
			args = append(args, "-pwd", pwd)
		}
	}

	return
}
