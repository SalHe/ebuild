package cmd

import (
	"fmt"
	"github.com/gookit/color"
	"path/filepath"

	"github.com/SalHe/ebuild/deps"
	"github.com/spf13/cobra"
)

var (
	absolutePath bool
	showPassword bool
)

var listCmd = cobra.Command{
	Use:    "list",
	Short:  "列出已搜寻到的源文件。",
	PreRun: loadConfiguration,
	Run: func(cmd *cobra.Command, args []string) {
		fmt.Println()
		if len(deps.ESrcs) > 0 {
			color.Greenln("已为您找到以下源文件：")
		} else {
			color.Redln("未能找到匹配模式的源文件哦。")
		}

		for _, srcPath := range deps.ESrcs {
			var out string
			rel, _ := filepath.Rel(deps.BuildDir, srcPath)
			if absolutePath {
				out = srcPath
			} else {
				out = rel
			}
			fmt.Print(out)

			if showPassword {
				pwd := deps.PasswordResolver.Resolve(rel)
				if len(pwd) > 0 {
					fmt.Print("\t\t\t")
					color.Grayp("  ---- 密码：" + pwd)
				}
			}

			fmt.Println()
		}
	},
}

func init() {
	listCmd.Flags().BoolVar(&absolutePath, "abs", false, "使用绝对路径展示")
	listCmd.Flags().BoolVar(&showPassword, "password", false, "显示密码")
}
