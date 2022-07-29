package cmd

import (
	"errors"
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/sources"
	"github.com/SalHe/ebuild/toolchain"
	"github.com/SalHe/ebuild/utils"
	"github.com/gookit/color"
	"github.com/spf13/cobra"
	"io/fs"
	"os"
	"path/filepath"
	"strconv"
	"strings"
)

const enableEclConcurrency = false

var buildCmd = cobra.Command{
	Use:     "build [构建目标...]",
	Short:   "根据配置构建目标",
	Long:    "根据配置构建目标，未单独配置的目标将采用全局编译器配置，并使用默认输出名等。",
	PreRunE: loadConfiguration,
	RunE: func(cmd *cobra.Command, args []string) error {
		_ = os.MkdirAll(deps.OutputDir, fs.ModePerm)

		// 并行应该会影响ECL区分到底是哪个窗口
		if //goland:noinspection GoBoolExpressions
		!enableEclConcurrency {
			concurrencyCount = 1
		}
		eSrcs := deps.ESrcs
		if len(args) > 0 {
			eSrcs = sources.FilterESrcs(eSrcs, args)
		}
		tasks := utils.NewLiveTasks(len(eSrcs), int(concurrencyCount))
		tasks.Header(func(over bool, allOk bool) string {
			if !over {
				return "正在编译...\n\n"
			}

			if allOk {
				return "任务已编译完成 \n\n"
			}

			return color.Red.Render("部分编译出错 \n\n")
		})
		tasks.OnPreExec(func(id int, te *utils.TasksExecutor, update utils.UpdateDisplayFunc) error {
			src := eSrcs[id]
			pwd := deps.PasswordResolver.Resolve(src.Source)
			args := src.CompileArgs(deps.OutputDir, pwd)
			cmdLine := color.Gray.Render(toolchain.Ecl(), " ", strings.Join(args, " "))
			update(fmt.Sprintf("[等待编译][%v] -> [%v] %v", src.DisplayName(), src.OutputPath(deps.OutputDir), cmdLine))
			return nil
		})
		tasks.OnExec(func(id int, te *utils.TasksExecutor, update utils.UpdateDisplayFunc) error {
			src := eSrcs[id]
			outputPath := src.OutputPath(deps.OutputDir)
			updateByTemplate := func(c string) {
				update(fmt.Sprintf("[正在编译][%v] -> [%v] %v", src.DisplayName(), outputPath, c))
			}

			pwd := deps.PasswordResolver.Resolve(src.Source)
			args := src.CompileArgs(deps.OutputDir, pwd)
			_ = os.MkdirAll(filepath.Dir(src.OutputPath(deps.OutputDir)), fs.ModePerm)

			errorTips := ""
			compileOk := true

			exec := toolchain.NewEclCmd(toolchain.Ecl(), args...)
			exec.OnLog(func(log string) {
				updateByTemplate(log)
			})
			exec.OnError(func(err string) {
				updateByTemplate(color.Red.Render(err))
				errorTips += "\n\t" + err
				compileOk = false
			})
			exec.OnExit(func(code int) {
				eclError := toolchain.EclError(code)
				if !eclError.IsOk() {
					tips := toolchain.EclErrorTips(eclError)
					errorTips += "\n\t" + tips + "(错误代码：" + strconv.Itoa(int(eclError)) + ")"
					compileOk = false
				}
			})
			exec.Exec()
			exec.Wait()

			if compileOk {
				update(color.Green.Sprintf("✔ [%v] -> [%v]", src.DisplayName(), outputPath))
				return nil
			} else {
				update(color.Red.Sprintf("❌ [%v] -> [%v] %v", src.DisplayName(), outputPath, errorTips))
				return errors.New("编译失败")
			}
		})
		tasks.StartAndWait()

		if tasks.AllOk() {
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
