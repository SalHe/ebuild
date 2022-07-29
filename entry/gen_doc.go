package main

import (
	"github.com/SalHe/ebuild/cmd"
	"github.com/spf13/cobra/doc"
)

func main() {
	doc.GenMarkdownTree(cmd.RootCommand(), "./docs/cli")
}
