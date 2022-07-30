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
exclude-builds:
  - './scripts/**/*.e' # 脚本文件不纳入'ebuild build'命令中进行自动构建
e2txt:
  name-style: 中文
  generate-e: true
build:
  compiler: 独立编译
targets:
  - name: Windows窗口程序示例
    description: 这是一个特别简单的窗口程序，里面有个按钮，你点击后会弹出提示。
    source: ./Windows窗口程序.e # 相对于项目根路径
    # 因为没有为该目标指定编译方式，所以会默认采用工程配置'build.compiler'中的编译方式
    output: Windows窗口程序示例.exe
    package: false # 不是易包

  - name: Windows控制台程序——静态编译版
    description: 这是一个简单的控制台程序，会在标准输出输出一句问候。
    source: ./Windows控制台程序.e
    output: Windows控制台程序示例——静态编译版.exe
    build:
      compiler: 静态编译

  - name: Windows控制台程序——黑月版
    description: 这是一个简单的控制台程序，会在标准输出输出一句问候。
    source: ./Windows控制台程序.e
    output: Windows控制台程序示例——黑月版.exe
    build:
      compiler: 黑月编译
`[1:] // 删除首空行

const (
	defaultGitignore = `# 恢复出来的易语言源文件和密码文件不纳入版本控制
*.recover.e
ebuild.pwd.yaml

# 易语言产生的备份源码文件
*.bak


# 编译输出
ebuild-out/`

	defaultReadMe = `# ebuild-example
	
	这是由[ebuild](https://github.com/SalHe/ebuild)创建的示例工程。
`
)

var initCmd = cobra.Command{
	Use:   "init",
	Short: "初始化一个工程。",
	Run: func(cmd *cobra.Command, args []string) {
		configFile := path.Join(deps.ProjectDir, "ebuild.yaml")
		fmt.Printf("配置文件路径：%s\n", configFile)

		if _, err := os.Stat(deps.ProjectDir); errors.Is(err, fs.ErrNotExist) {
			color.Yellowf("项目目录不存在，将为您创建：%s\n", deps.ProjectDir)
			if err := os.MkdirAll(deps.ProjectDir, os.ModePerm); err != nil {
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
			os.WriteFile(path.Join(deps.ProjectDir, ".gitignore"), []byte(defaultGitignore), 0666)
			os.WriteFile(path.Join(deps.ProjectDir, "ebuild.pwd.yaml"), []byte(`Windows窗口程序--带密码.e: 123`), 0666)
			os.WriteFile(path.Join(deps.ProjectDir, "README.md"), []byte(defaultReadMe), 0666)

			color.Greenf("成功创建配置文件。")
		} else {
			fmt.Println("配置文件已存在。")
		}
	},
}
