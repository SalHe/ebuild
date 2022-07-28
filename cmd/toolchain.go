package cmd

import (
	"github.com/SalHe/ebuild/toolchain"
	"github.com/gookit/color"
	"github.com/spf13/cobra"
)

var toolchainCommand = cobra.Command{
	Use:   "toolchain",
	Short: "工具链相关",
	Run: func(cmd *cobra.Command, args []string) {
		toolchains := []struct {
			path string
			name string
		}{
			{toolchain.ELang(), "易语言"},
			{toolchain.E2Txt(), "e2txt"},
			{toolchain.Ecl(), "ecl"},
		}
		for _, tc := range toolchains {
			if len(tc.path) > 0 {
				color.Greenf("✔ %v: %v\n", tc.name, tc.path)
			} else {
				color.Redf("❌ %v: %v\n", tc.name, "未找到工具")
			}
		}
	},
}
