package cmd

import (
	"errors"
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/hooks"
	"github.com/SalHe/ebuild/sources"
	"github.com/SalHe/ebuild/toolchain"
	"github.com/SalHe/ebuild/utils/env"
	"github.com/SalHe/ebuild/utils/tasks"
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
	Use:   "build [构建目标...]",
	Short: "根据配置构建目标",
	Long:  "根据配置构建目标，未单独配置的目标将采用全局编译器配置，并使用默认输出名等。",
	PreRunE: checkTool(
		func() string { return toolchain.Ecl() },
		"ecl",
		loadConfiguration,
	),
	RunE: runBuild,
}

func init() {
	if enableEclConcurrency {
		buildCmd.Flags().Uint8VarP(&concurrencyCount, "concurrency", "c", 8, "最大并行编译的个数.")
	}
}
func runBuild(cmd *cobra.Command, args []string) error {
	_ = os.MkdirAll(deps.OutputDir, fs.ModePerm)

	// 并行应该会影响ECL区分到底是哪个窗口
	if //goland:noinspection GoBoolExpressions
	!enableEclConcurrency {
		concurrencyCount = 1
	}
	eSrcs := deps.ESrcs
	if len(args) > 0 {
		eSrcs = sources.FilterESrcs(eSrcs, sources.FilterTargets(args))
	}
	eSrcs = sources.FilterESrcs(eSrcs, sources.FilterRmNoBuild())

	liveTasks := tasks.NewLiveTasks(len(eSrcs), int(concurrencyCount))
	liveTasks.Header(func(over bool, allOk bool) string {
		if !over {
			return "正在编译...\n\n"
		}

		if allOk {
			return "任务已编译完成 \n\n"
		}

		return color.Red.Render("部分编译出错 \n\n")
	})
	liveTasks.OnPreRun(onPreRunCompileSource(eSrcs))
	liveTasks.OnRun(onRunCompileSource(eSrcs))
	liveTasks.StartAndWait()

	if liveTasks.AllOk() {
		return nil
	}
	return errors.New("编译失败")
}

func onBuildHooks(src *sources.Source, period hooks.EBuildPeriod, update tasks.UpdateDisplayFunc) {
	batContent := src.Hooks[string(period)]
	if len(batContent) <= 0 {
		return
	}

	originalUpdate := update
	update = func(display string) {
		originalUpdate(color.Yellow.Sprintf("[%v][%v] -> [%v] %v", period, src.DisplayName(), src.OutputPath(deps.OutputDir), display))
	}
	update(color.Yellow.Render("准备执行编译周期脚本..."))

	batPath, err := tempBat(batContent)
	defer os.Remove(batPath)
	if err != nil {
		return
	}

	environ := env.NewEnv()
	environ.ForBuild(src.AbsPath(), src.OutputPath(deps.OutputDir), src.TargetType(), period)

	exec := toolchain.NewExec(batPath)
	exec.SetGbk(false)
	exec.LoadEnv(environ.EnvMap())
	exec.OnStdout(toolchain.ReportFunc(update))
	exec.OnStderr(func(s string) {
		update(color.Red.Render(s))
	})
	exec.ForwardStdin()
	exec.Exec()
	exec.Wait()
}

func onPreRunCompileSource(eSrcs []*sources.Source) func(id int, te *tasks.TasksExecutor, update tasks.UpdateDisplayFunc) error {
	return func(id int, te *tasks.TasksExecutor, update tasks.UpdateDisplayFunc) error {
		src := eSrcs[id]
		onBuildHooks(src, hooks.PeriodPreBuild, update)

		pwd := deps.PasswordResolver.Resolve(src.Source)
		args := src.CompileArgs(deps.OutputDir, pwd)
		cmdLine := color.Gray.Render(toolchain.Ecl(), " ", strings.Join(args, " "))
		update(fmt.Sprintf("[等待编译][%v] -> [%v] %v", src.DisplayName(), src.OutputPath(deps.OutputDir), cmdLine))
		return nil
	}
}

func onRunCompileSource(eSrcs []*sources.Source) func(id int, te *tasks.TasksExecutor, update tasks.UpdateDisplayFunc) error {
	return func(id int, te *tasks.TasksExecutor, update tasks.UpdateDisplayFunc) error {
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
			onBuildHooks(src, hooks.PeriodPostBuild, update)
			update(color.Green.Sprintf("✔ [%v] -> [%v]", src.DisplayName(), outputPath))
			return nil
		} else {
			update(color.Red.Sprintf("❌ [%v] -> [%v] %v", src.DisplayName(), outputPath, errorTips))
			return errors.New("编译失败")
		}
	}
}
