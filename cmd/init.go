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

const defaultEBuildYaml = `project:
  name: ebuild-example
  version: "1.0"
  description: 这是由ebuild创建的示例工程。
  author: SalHe
  repository: https://github.com/SalHe/ebuild
  homepage: https://github.com/SalHe
excludes:
  - '**/*.recover.e'
  - '**/*.ecode/*.e'
  - '**/*.代码/*.e'
exclude-builds:
  - './scripts/**/*.e' # 脚本文件不纳入'ebuild build'命令中进行自动构建
includes:
  - '**/*.e'
e2txt:
  name-style: 中文
  generate-e: true
build:
  compiler: 独立编译
`

const (
	defaultGitignore = `
# 恢复出来的易语言源文件和密码文件不纳入版本控制
*.recover.e
ebuild.pwd.yaml
**/*.ecode/log
**/*.ecode/日志

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
			gitignoreFilePath := path.Join(deps.ProjectDir, ".gitignore")
			bytes, _ := os.ReadFile(gitignoreFilePath)
			os.WriteFile(gitignoreFilePath, append(bytes, []byte(defaultGitignore)...), 0666)
			os.WriteFile(path.Join(deps.ProjectDir, "ebuild.pwd.yaml"), []byte(`Windows窗口程序--带密码.e: 123`), 0666)
			os.WriteFile(path.Join(deps.ProjectDir, "README.md"), []byte(defaultReadMe), 0666)

			color.Greenln("成功创建配置文件。")
			color.Greenln("有关工程的配置可以参见ebuild示例代码：https://github.com/SalHe/ebuild/tree/master/examples")
			color.Greenln("此外，您也可以去ebuild官方网站查看更加详细的文档：https://salhe.github.io/ebuild/")
		} else {
			fmt.Println("配置文件已存在。")
		}
	},
}
