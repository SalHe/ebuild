package cmd

import (
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/gookit/color"
	"github.com/spf13/cobra"
	"path/filepath"
)

var (
	absolutePath bool
	showPassword bool
)

var infoCmd = cobra.Command{
	Use:    "info",
	Short:  "列出项目的相关信息。",
	PreRun: loadConfiguration,
	Run: func(cmd *cobra.Command, args []string) {
		showProjectInfo()
		fmt.Println()
		showE2TxtConfig()
		fmt.Println()
		showSources()
	},
}

func showE2TxtConfig() {
	color.BgWhite.Println("    E2TXT：      ")
	fmt.Printf("风格：%s\n", deps.C.E2Txt.Style)
}

func showProjectInfo() {
	color.BgWhite.Println("    当前项目信息：      ")
	fmt.Printf("项目：%s\n", deps.C.Project.Name)
	fmt.Printf("介绍：%s\n", deps.C.Project.Description)
	fmt.Printf("版本：%s\n", deps.C.Project.Version)
	fmt.Printf("作者：%s\n", deps.C.Project.Author)
	fmt.Printf("仓库：%s\n", deps.C.Project.Repository)
	fmt.Printf("主页：%s\n", deps.C.Project.Homepage)
}

func showSources() {
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
}

func init() {
	infoCmd.Flags().BoolVar(&absolutePath, "abs", false, "使用绝对路径展示")
	infoCmd.Flags().BoolVar(&showPassword, "password", false, "显示密码")
}
