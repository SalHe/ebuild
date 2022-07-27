package config

type CompilerType string

const (
	CompilerBlackMoon    = "黑月"
	CompilerBlackMoonAsm = "黑月汇编"
	CompilerBlackMoonCPP = "黑月C++"
	CompilerBlackMoonMFC = "黑月MFC"
	CompilerStatic       = "静态编译"
	CompilerIndependent  = "独立编译"
)

type Target struct {
	Source             string `mapstructure:"source"` // 约定为相对于 ebuild.yaml 的相对路径
	Output             string `mapstructure:"output"`
	Description        string `mapstructure:"description"`
	Build              *Build `mapstructure:"build"`
	CompileConfig      string `mapstructure:"compile-config"`
	CompileDescription string `mapstructure:"compile-description"`
	Package            bool   `mapstructure:"package"` // 是否为易包
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
