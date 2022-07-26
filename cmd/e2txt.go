package cmd

import (
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/toolchain"
	"github.com/SalHe/ebuild/utils"
	"github.com/gookit/color"
	"github.com/spf13/cobra"
	"path/filepath"
	"strings"
)

var concurrencyCount uint8

var e2txtCmd = cobra.Command{
	Use:    "e2txt",
	Short:  "将项目中包含的源文件按照约定配置转换成文本式的代码。",
	PreRun: loadConfiguration,
	Run:    runE2Txt,
}

func init() {
	e2txtCmd.Flags().Uint8VarP(&concurrencyCount, "concurrency", "c", 5, "最大并行转换文件的个数.")
}

func runE2Txt(cmd *cobra.Command, args []string) {
	over := false
	allOk := true

	// 准备多行动态刷新
	liveLines := utils.NewLiveLines(len(deps.ESrcs))
	liveLines.Header(func() string {
		if over {
			if allOk {
				return "转换已完成！\n\n"
			} else {
				return color.Red.Render("转换结束，部分源码转换失败，请注意查看！\n\n")
			}
		} else {
			return "正在为您转换源文件，请耐心等候：\n\n"
		}
	})
	liveLines.Start()

	// 准备执行任务
	tasksExecutor := utils.NewTasksExecutor(len(deps.ESrcs), int(concurrencyCount))
	tasksExecutor.OnPreExec = func(id int, te *utils.TasksExecutor) {
		src := deps.ESrcs[id]
		srcRel, _ := filepath.Rel(deps.BuildDir, src)
		liveLines.Update(id, fmt.Sprintf("[等待中][%s]", srcRel))
	}
	tasksExecutor.OnExec = func(id int, te *utils.TasksExecutor) {
		update := func(o string) { liveLines.Update(id, o) }
		errorOccurs := func() { allOk = false }
		convertE2Txt(update, errorOccurs, deps.ESrcs[id])
	}
	tasksExecutor.Start()

	tasksExecutor.Wait()
	liveLines.Stop()
}

func convertE2Txt(out func(string), errorOccurs func(), src string) {
	ecode := utils.ECodeDir(src)
	srcRel, _ := filepath.Rel(deps.BuildDir, src)
	eCodeRel, _ := filepath.Rel(deps.BuildDir, ecode)

	args := deps.C.E2Txt.ArgsE2Txt(src, ecode)

	execE2TxtCmd(out, errorOccurs, srcRel, args, eCodeRel)
}

func execE2TxtCmd(out func(string), errorOccurs func(), srcRel string, args []string, eCodeRel string) {
	wrong := false
	errTips := ""
	cmdOver := make(chan interface{})

	out(fmt.Sprintf("[开始转换][%s]: %s", srcRel, color.Gray.Render(toolchain.E2Txt()+" "+strings.Join(args, " "))))

	cmd := toolchain.NewE2TxtCmd(toolchain.E2Txt(), args...)
	cmd.OnLog = func(log string) { out(fmt.Sprintf("[转换中][%s]: %s", srcRel, log)) }
	cmd.OnError = func(err string) {
		wrong = true
		errTips += "\n\t" + err // 错误提示可以叠加，方便查看过程中出现的所有错误
		out(fmt.Sprintf("[转换出错][%s]: 转换出错 %v", srcRel, err))
	}
	cmd.OnOutDir = func(outDir string) { out(fmt.Sprintf("[转换完成][%s]: 已保存到%s", srcRel, outDir)) }
	cmd.OnOver = func() { cmdOver <- nil }
	cmd.Exec()

	<-cmdOver

	if !wrong {
		out(fmt.Sprintf(color.Green.Sprintf("✔ [%s] -> [%s]", srcRel, eCodeRel)))
	} else {
		errorOccurs()
		out(fmt.Sprintf(color.Red.Sprintf("❌ [%s] -> [%s] %v", srcRel, eCodeRel, errTips)))
	}
}
