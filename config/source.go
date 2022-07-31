package config

import "strings"

type CompilerType string

func (c *CompilerType) IsBM() bool {
	return strings.HasPrefix(string(*c), "黑月")
}

const (
	CompilerBlackMoon    = "黑月编译"
	CompilerBlackMoonAsm = "黑月汇编"
	CompilerBlackMoonCPP = "黑月C++"
	CompilerBlackMoonMFC = "黑月MFC"
	CompilerStatic       = "静态编译"
	CompilerIndependent  = "独立编译"
)

type Target struct {
	Name               string            `mapstructure:"name"`        // 目标名称，只是一个简单名字介绍
	Description        string            `mapstructure:"description"` // 目标描述
	Source             string            `mapstructure:"source"`      // 约定为相对于 ebuild.yaml 的相对路径
	Output             string            `mapstructure:"output"`
	Build              *Build            `mapstructure:"build"`
	CompileConfig      string            `mapstructure:"compile-config"`
	CompileDescription string            `mapstructure:"compile-description"`
	Package            bool              `mapstructure:"package"` // 是否为易包
	Hooks              map[string]string `mapstructure:"hooks"`
}

func (c *CompilerType) Args(config string, description string) (args []string) {
	switch *c {
	case CompilerBlackMoon:
		args = []string{"-bm"}
	case CompilerBlackMoonAsm:
		args = []string{"-bm0"}
	case CompilerBlackMoonCPP:
		args = []string{"-bm1"}
	case CompilerBlackMoonMFC:
		args = []string{"-bm2"}
	case CompilerStatic:
		args = []string{"-s"}
	case CompilerIndependent:
		args = []string{"-d"}
	}

	if c.IsBM() {
		if len(config) > 0 {
			args = append(args, "-bmcfg", config)
		}
		if len(description) > 0 {
			args = append(args, "-bmdesc", description)
		}
	}

	return
}
