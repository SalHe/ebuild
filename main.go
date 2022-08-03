package main

import (
	"github.com/SalHe/ebuild/cmd"
	_ "github.com/SalHe/ebuild/toolchain"
)

//go:generate go run ./entry/gen_version.go

func main() {
	cmd.Execute()
}
