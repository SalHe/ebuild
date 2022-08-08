package env

import (
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/hooks"
	"github.com/SalHe/ebuild/sources"
	"github.com/SalHe/ebuild/toolchain"
	"os"
	"path/filepath"
)

//goland:noinspection ALL
const (
	// toolchain
	EnvEBuildExecutable = "EBUILD_EXECUTABLE_PATH"
	EnvElangDir         = "ELANG_DIR"
	EnvEclDir           = "ECL_DIR"
	EnvE2TxtDir         = "E2TXT_DIR"

	// project
	EnvEBuildProjectRootDir   = "EBUILD_PROJECT_ROOT_DIR"
	EnvEBuildProjectOutputDir = "EBUILD_PROJECT_OUTPUT_DIR"

	// hooks
	EnvEBuildPeriod     = "EBUILD_PERIOD"
	EnvEBuildSourceFile = "EBUILD_SOURCE_FILE"
	EnvEBuildTargetFile = "EBUILD_TARGET_FILE"
	EnvEBuildTargetType = "EBUILD_TARGET_TYPE"
)

type Env struct {
	env map[string]string
}

func (e *Env) loadBasicEnv() {
	p, _ := filepath.Abs(os.Args[0])
	e.Set(EnvEBuildExecutable, p)
	e.Set(EnvElangDir, filepath.Dir(toolchain.ELang()))
	e.Set(EnvEclDir, filepath.Dir(toolchain.Ecl()))
	e.Set(EnvE2TxtDir, filepath.Dir(toolchain.E2Txt()))

	e.Set(EnvEBuildProjectRootDir, deps.ProjectDir)
	e.Set(EnvEBuildProjectOutputDir, deps.OutputDir)
}

func (e *Env) Set(name, value string) {
	e.env[name] = value
}

func (e *Env) ForBuild(sourceFile string, targetFile string, targetType sources.EBuildTargetType, buildPeriod hooks.EBuildPeriod) {
	e.Set(EnvEBuildSourceFile, sourceFile)
	e.Set(EnvEBuildTargetFile, targetFile)
	e.Set(EnvEBuildTargetType, targetType.String())
	e.Set(EnvEBuildPeriod, string(buildPeriod))
}

func (e *Env) EnvMap() map[string]string {
	return e.env
}

func NewEnv() *Env {
	e := &Env{
		env: make(map[string]string),
	}
	e.loadBasicEnv()
	return e
}