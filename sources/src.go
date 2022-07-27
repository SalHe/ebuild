package sources

import (
	"github.com/SalHe/ebuild/config"
	"github.com/SalHe/ebuild/utils"
	"path/filepath"
	"strings"
)

type Source struct {
	*config.Target

	buildPath string
	fromYaml  bool
}

func FromYAML(src *config.Target, buildPath string) *Source {
	return &Source{
		Target:    src,
		buildPath: buildPath,

		fromYaml: true,
	}
}

func FromPath(srcPath string, build *config.Build, buildPath string) *Source {
	return &Source{
		Target: &config.Target{
			Source:  srcPath,
			Package: false,
			Build:   build,
		},
		buildPath: buildPath,
		fromYaml:  false,
	}
}

func (s *Source) CompileArgs(pwd string) (args []string) {
	args = append(args, s.Target.Source, s.Output)

	if s.Package {
		args = append(args, "-p")
	} else {
		args = append(args, s.Build.Compiler.Args()...)
		if strings.HasPrefix(string(s.Build.Compiler), "-bm") {
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

func (s *Source) AbsPath() string {
	return filepath.Join(s.buildPath, s.Target.Source)
}

func (s *Source) FromConfig() bool {
	return s.fromYaml
}

func (s *Source) ECodeDir() string {
	return utils.ECodeDir(s.AbsPath())
}

func (s *Source) RecoverESrcPath() string {
	dir, name, ext := utils.FilePathElements(s.AbsPath())
	return filepath.Join(dir, name+".recover"+ext)
}
