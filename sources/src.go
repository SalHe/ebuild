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

func FromYAML(src *config.Target, build *config.Build, buildPath string) *Source {
	s := &Source{
		Target:    src,
		buildPath: buildPath,

		fromYaml: true,
	}
	if s.Build == nil {
		s.Build = build
	}
	return s
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

func (s *Source) CompileArgs(outputDir string, pwd string) (args []string) {
	args = append(args, "make", s.AbsPath(), s.OutputPath(outputDir))

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

func (s *Source) OutputPath(outputDir string) string {
	dir, name, _ := utils.FilePathElements(s.AbsPath())
	relDir, _ := filepath.Rel(s.buildPath, dir)
	if s.Output != "" {
		name = s.Output
	}
	return filepath.Join(outputDir, relDir, name)
}

func (s *Source) DisplayName() string {
	if len(s.Name) <= 0 {
		return s.Source
	}
	return s.Name
}
