package sources

import (
	"encoding/binary"
	"github.com/SalHe/ebuild/config"
	"github.com/SalHe/ebuild/utils"
	"io"
	"os"
	"path/filepath"
)

type Source struct {
	*config.Target

	projectDir string
	fromYaml   bool

	sourceType ESourceType
	targetType EBuildTargetType
}

func newSource(src *config.Target, build *config.Build, projectPath string, fromYaml bool) *Source {
	s := &Source{
		Target:     src,
		projectDir: projectPath,

		fromYaml: fromYaml,
	}
	if s.Build == nil {
		s.Build = build
	}
	s.init()
	return s
}

func FromYAML(src *config.Target, build *config.Build, projectPath string) *Source {
	return newSource(src, build, projectPath, true)
}

func FromPath(srcPath string, build *config.Build, projectDir string) *Source {
	return newSource(&config.Target{
		Source:  srcPath,
		Package: false,
		Build:   build,
	}, build, projectDir, false)
}

func (s *Source) CompileArgs(outputDir string, pwd string) (args []string) {
	args = append(args, "make", s.AbsPath(), s.OutputPath(outputDir))

	if s.Package {
		args = append(args, "-p")
	} else {
		if s.TargetType() != EBuildTargetWindowsECom && s.TargetType() != EBuildTargetLinuxECom && s.Build != nil {
			args = append(args, s.Build.Compiler.Args(s.CompileConfig, s.CompileDescription)...)
		}
		if len(pwd) > 0 {
			args = append(args, "-pwd", pwd)
		}
	}

	return
}

func (s *Source) AbsPath() string {
	if filepath.IsAbs(s.Source) {
		return s.Source
	}
	return filepath.Join(s.projectDir, s.Target.Source)
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
	if filepath.IsAbs(s.Output) {
		return s.Output
	}
	dir, name, _ := utils.FilePathElements(s.AbsPath())
	relDir, _ := filepath.Rel(s.projectDir, dir)
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

func (s *Source) Match(target string) bool {
	if target == s.Name {
		return true
	}
	return s.AbsPath() == filepath.Join(s.projectDir, target)
}

func (s *Source) init() {
	if file, err := os.OpenFile(s.AbsPath(), os.O_RDONLY, os.ModePerm); err != nil {
		s.sourceType = -1
		s.targetType = -1
		return
	} else {
		file.Seek(124, io.SeekStart)
		binary.Read(file, binary.LittleEndian, &s.sourceType)
		file.Seek(132, io.SeekStart)
		binary.Read(file, binary.LittleEndian, &s.targetType)
		file.Close()
	}
}

func (s *Source) SourceType() ESourceType {
	return s.sourceType
}

func (s *Source) TargetType() EBuildTargetType {
	return s.targetType
}
