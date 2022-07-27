package cmd

import (
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/sources"
	"github.com/gookit/color"
	"github.com/spf13/cobra"
	"os"
)

var cleanCmd = cobra.Command{
	Use:   "clean",
	Short: "清理恢复的源码等工程中间文件。",
	Long: `清理工程中间文件，包括：
  1. txt2e 中从文本格式代码恢复出来的易语言源文件(*.recover.e，只清理包含在工程源文件和目标中的恢复代码)；
* 2. e2txt 中从易语言源文件转换成的文本格式代码的文件夹(*.ecode，只清理包含在工程源文件和目标中的恢复代码)。（默认不清理）
`,
	PreRunE: loadConfiguration,
	Run: func(cmd *cobra.Command, args []string) {
		cleanTasks := []struct {
			description string
			preRun      func() bool
			run         func(src *sources.Source)
			disabled    bool
			singleRun   bool
		}{
			{
				description: "*.recover.e",
				run: func(src *sources.Source) {
					fmt.Println("正在删除 " + src.RecoverESrcPath())
					_ = os.Remove(src.RecoverESrcPath())
				},
			},

			{
				description: "*.ecode",
				disabled:    !cleanECodeDir,
				preRun: func() bool {
					color.Redln("您正在尝试清理文本格式的代码，可能会造成数据丢失，请问您确认吗？（输入 Y 确定）")
					var line string
					fmt.Scanln(&line)
					return line == "Y"
				},
				run: func(src *sources.Source) {
					color.Redln("正在删除 " + src.ECodeDir())
					_ = os.RemoveAll(src.ECodeDir())
				},
			},

			{
				description: "编译结果",
				singleRun:   true,
				run: func(src *sources.Source) {
					fmt.Println("正在删除 " + deps.OutputDir)
					_ = os.RemoveAll(deps.OutputDir)
				},
			},
		}

		fmt.Println()
		for _, task := range cleanTasks {
			if task.disabled {
				continue
			}

			color.Greenln("正在清理 " + task.description + " ...")
			if !force && task.preRun != nil && !task.preRun() {
				color.Yellowln("操作已取消。")
				continue
			}

			if task.singleRun {
				task.run(nil)
			} else {
				for _, src := range deps.ESrcs {
					task.run(src)
				}
			}
			fmt.Println("清理完成")
			fmt.Println()
		}
	},
}

var (
	cleanECodeDir bool
	force         bool
)

func init() {
	cleanCmd.Flags().BoolVar(&cleanECodeDir, "ecode", false, "清理 .ecode 文件夹")
	cleanCmd.Flags().BoolVarP(&force, "force", "f", false, "强制同意。针对危险的操作，ebuild会尝试询问您是否确定，但是如果您开启了此标志，则ebuild认为您坚持对所有危险操作继续执行。")
}
