package cmd

import (
	"errors"
	"fmt"
	"github.com/gookit/color"
	"io/fs"
	"os"
	"path"

	"github.com/SalHe/ebuild/deps"
	"github.com/spf13/cobra"
)

var defaultEBuildYaml = `
project:
    name: ebuild-example
    version: "1.0"
    description: 这是由ebuild创建的示例工程。
    author: SalHe
    repository: https://github.com/SalHe/ebuild
    homepage: https://github.com/SalHe
excludes:
    - '**/*.recover.e'
    - '**/*.ecode/**.e'
    - '**/*.代码/**.e'
includes:
    - '**/*.e'
e2txt:
    name-style: 中文
	generate-e: true
`[1:] // 删除首空行

var initCmd = cobra.Command{
	Use:   "init",
	Short: "初始化一个工程。",
	Run: func(cmd *cobra.Command, args []string) {
		configFile := path.Join(deps.BuildDir, "ebuild.yaml")
		fmt.Printf("配置文件路径：%s\n", configFile)

		if _, err := os.Stat(deps.BuildDir); errors.Is(err, fs.ErrNotExist) {
			color.Yellowf("项目目录不存在，将为您创建：%s\n", deps.BuildDir)
			if err := os.MkdirAll(deps.BuildDir, os.ModePerm); err != nil {
				fmt.Println("创建目录失败！")
				os.Exit(1)
			}
		}

		if _, err := os.Stat(configFile); errors.Is(err, fs.ErrNotExist) {
			if _, err := os.Create(configFile); err != nil {
				color.Redf("创建配置文件失败: %s\n", configFile)
				return
			}

			if err := os.WriteFile(configFile, []byte(defaultEBuildYaml), 0666); err != nil {
				color.Redln("创建配置文件失败！")
				os.Exit(1)
			}
			os.WriteFile(path.Join(deps.BuildDir, ".gitignore"), []byte(`ebuild.pwd.yaml`), 0666)
			os.WriteFile(path.Join(deps.BuildDir, "ebuild.pwd.yaml"), []byte(`a.e: '123456pwd'`), 0666)

			color.Greenf("成功创建配置文件。")
		} else {
			fmt.Println("配置文件已存在。")
		}
	},
}
