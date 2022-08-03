package toolchain

import (
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/utils"
	"golang.org/x/sys/windows/registry"
	"os"
	"os/exec"
	"path/filepath"
)

var (
	elang string
	ecl   string
	e2txt string
)

func fetchELangPath() string {
	l := lookup("e")
	if len(l) > 0 {
		return l
	}

	key, err := registry.OpenKey(registry.CURRENT_USER, "Software\\FlySky\\E\\Install", registry.READ)
	defer key.Close()
	if err != nil {
		return ""
	}

	path, _, _ := key.GetStringValue("Path")
	if len(path) > 0 {
		return path[:len(path)-4] + "e.exe"
	}
	return path
}

func lookupIn(subDir string, execName string) string {
	p := fmt.Sprintf("%v/.toolchain/%v/%v", deps.ProjectDir, subDir, execName)
	if utils.FileExists(p) {
		return p
	}

	p, _ = filepath.Abs(fmt.Sprintf("./.toolchain/%v/%v", subDir, execName))
	if utils.FileExists(p) {
		return p
	}

	dir, _ := filepath.Abs(filepath.Dir(os.Args[0]))
	p, _ = filepath.Abs(fmt.Sprintf("%v/.toolchain/%v/%v", dir, subDir, execName))
	if utils.FileExists(p) {
		return p
	}

	p, _ = exec.LookPath(execName)
	if utils.FileExists(p) {
		p, _ = filepath.Abs(p)
		return p
	}

	return ""
}

func lookup(execName string) string {
	return lookupIn(execName, execName+".exe")
}

func Ecl() string {
	if ecl == "" {
		ecl = lookup("ecl")
	}
	return ecl
}

func ELang() string {
	if elang == "" {
		elang = fetchELangPath()
	}
	return elang
}

func E2Txt() string {
	if e2txt == "" {
		e2txt = lookup("e2txt")
	}
	return e2txt
}
