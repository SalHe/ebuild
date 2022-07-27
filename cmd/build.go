package cmd

import (
	"errors"
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/toolchain"
	"github.com/SalHe/ebuild/utils"
	"github.com/gookit/color"
	"github.com/spf13/cobra"
	"io/fs"
	"os"
	"strings"
)

const enableEclConcurrency = false

var buildCmd = cobra.Command{
	Use:     "build",
	Short:   "根据配置构建目标",
	Long:    "根据配置构建目标，未单独配置的目标将采用全局编译器配置，并使用默认输出名等。",
	PreRunE: loadConfiguration,
	RunE: func(cmd *cobra.Command, args []string) error {
		_ = os.MkdirAll(deps.OutputDir, fs.ModePerm)

		over := false
		allOk := true

		liveLines := utils.NewLiveLines(len(deps.ESrcs))
		liveLines.Header(func() string {
			if !over {
				return "正在编译...\n\n"
			}

			if allOk {
				return "任务已编译完成 \n\n"
			}

			return color.Red.Render("部分编译出错 \n\n")
		})
		liveLines.Start()

		// 并行应该会影响ECL区分到底是哪个窗口
		if //goland:noinspection GoBoolExpressions
		!enableEclConcurrency {
			concurrencyCount = 1
		}
		tasksExecutor := utils.NewTasksExecutor(len(deps.ESrcs), int(concurrencyCount))
		tasksExecutor.OnPreExec = func(id int, te *utils.TasksExecutor) {
			src := deps.ESrcs[id]
			pwd := deps.PasswordResolver.Resolve(src.Source)
			args := src.CompileArgs(deps.OutputDir, pwd)
			cmdLine := color.Gray.Render(toolchain.Ecl(), " ", strings.Join(args, " "))
			liveLines.Update(id, fmt.Sprintf("[等待编译][%v] -> [%v] %v", src.DisplayName(), src.OutputPath(deps.OutputDir), cmdLine))
		}
		tasksExecutor.OnExec = func(id int, te *utils.TasksExecutor) {
			src := deps.ESrcs[id]
			outputPath := src.OutputPath(deps.OutputDir)
			update := func(c string) {
				liveLines.Update(id, fmt.Sprintf("[正在编译][%v] -> [%v] %v", src.DisplayName(), outputPath, c))
			}

			pwd := deps.PasswordResolver.Resolve(src.Source)
			args := src.CompileArgs(deps.OutputDir, pwd)
			over := make(chan interface{})

			errorTips := ""
			compileOk := true

			exec := toolchain.NewEclCmd(toolchain.Ecl(), args...)
			exec.OnLog(func(log string) {
				update(log)
			})
			exec.OnError(func(err string) {
				update(color.Red.Render(err))
				errorTips += "\n\t" + err
				compileOk = false
				allOk = false
			})
			exec.OnOver(func() { over <- nil })
			exec.Exec()

			<-over

			if compileOk {
				liveLines.Update(id, color.Green.Sprintf("✔ [%v] -> [%v]", src.DisplayName(), outputPath))
			} else {
				liveLines.Update(id, color.Red.Sprintf("❌ [%v] -> [%v] %v", src.DisplayName(), outputPath, errorTips))
			}
		}
		tasksExecutor.Start()

		tasksExecutor.Wait()
		over = true
		liveLines.Stop()

		if allOk {
			return nil
		}
		return errors.New("编译失败")
	},
}

func init() {
	if enableEclConcurrency {
		buildCmd.Flags().Uint8VarP(&concurrencyCount, "concurrency", "c", 8, "最大并行编译的个数.")
	}
}
