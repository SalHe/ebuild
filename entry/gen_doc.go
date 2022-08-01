package main

import (
	"github.com/SalHe/ebuild/cmd"
	"github.com/spf13/cobra/doc"
	"strings"
)

func main() {
	filePrerenderer := func(s string) string { return "" }
	linkHandler := func(s string) string {
		s = strings.TrimSuffix(s, ".md")
		s = strings.ReplaceAll(s, "_", "-")
		s = "/cli/#" + s
		return s
	}
	doc.GenMarkdownTreeCustom(cmd.RootCommand(), "./docs/cli", filePrerenderer, linkHandler)
}
