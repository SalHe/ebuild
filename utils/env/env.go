package env

import (
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
	EnvElangExecutable  = "ELANG_PATH"
	EnvEclExecutable    = "ECL_PATH"
	EnvE2TxtExecutable  = "E2TXT_PATH"

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
	e.Set(EnvElangExecutable, toolchain.ELang())
	e.Set(EnvEclExecutable, toolchain.Ecl())
	e.Set(EnvE2TxtExecutable, toolchain.E2Txt())
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
