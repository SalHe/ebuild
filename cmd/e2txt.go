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
	"path/filepath"
	"strings"
)

var concurrencyCount uint8

var e2txtCmd = cobra.Command{
	Use:     "e2txt",
	Short:   "将项目中包含的源文件按照约定配置转换成文本式的代码。",
	PreRunE: loadConfiguration,
	RunE:    runE2Txt,
}

var txt2eCmd = e2txtCmd

func init() {
	txt2eCmd.Use = "txt2e"
	txt2eCmd.Short = "将 .ecode 文件夹中的文本格式的易语言代码恢复成易语言二进制源文件。"

	cmds := []*cobra.Command{&e2txtCmd, &txt2eCmd}
	for _, cmd := range cmds {
		cmd.Flags().Uint8VarP(&concurrencyCount, "concurrency", "c", 5, "最大并行转换文件的个数.")
	}
}

func runE2Txt(cmd *cobra.Command, args []string) error {
	isE2Txt := cmd.Use != "txt2e"

	over := false
	allOk := true

	// 准备多行动态刷新
	liveLines := utils.NewLiveLines(len(deps.ESrcs))
	liveLines.Header(func() string {
		if !over {
			return "正在为您转换源文件，请耐心等候：\n\n"
		}

		if allOk {
			return "转换已完成！\n\n"
		}

		if isE2Txt {
			return color.Red.Render("转换结束，部分源码转换失败，请注意查看！\n\n")
		} else {
			return color.Red.Render("转换结束，恢复源码部分失败，请注意查看！\n\n")
		}
	})
	liveLines.Start()

	// 准备执行任务
	tasksExecutor := utils.NewTasksExecutor(len(deps.ESrcs), int(concurrencyCount))
	tasksExecutor.OnPreExec = func(id int, te *utils.TasksExecutor) {
		var src string
		if isE2Txt {
			src = deps.ESrcs[id].AbsPath()
		} else {
			src = deps.ESrcs[id].ECodeDir()
		}
		srcRel, _ := filepath.Rel(deps.ProjectDir, src)
		liveLines.Update(id, fmt.Sprintf("[等待中][%s]", srcRel))
	}
	tasksExecutor.OnExec = func(id int, te *utils.TasksExecutor) {
		src := deps.ESrcs[id]
		if len(deps.PasswordResolver.Resolve(src.Source)) <= 0 {
			update := func(o string) { liveLines.Update(id, o) }
			errorOccurs := func() { allOk = false }
			if isE2Txt {
				convertE2Txt(update, errorOccurs, src)
			} else {
				convertTxt2E(update, errorOccurs, src)
			}
		} else {
			srcRel, _ := filepath.Rel(deps.ProjectDir, src.AbsPath())
			dstRel, _ := filepath.Rel(deps.ProjectDir, src.ECodeDir())
			if isE2Txt {
				liveLines.Update(id, color.Yellow.Sprintf("❗[%s] -> [%s] 该文件设有密码，已跳过", srcRel, dstRel))
			} else {
				liveLines.Update(id, color.Yellow.Sprintf("❗[%s] -> [%s] 该文件设有密码，已跳过", dstRel, srcRel))
			}
		}
	}
	tasksExecutor.Start()

	tasksExecutor.Wait()
	over = true
	liveLines.Stop()

	if allOk {
		return nil
	}
	return errors.New("转换出错，具体信息请查看输出日志")
}

func convertTxt2E(out func(string), errorOccurs func(), src *sources.Source) {
	ecode := src.ECodeDir()
	recoverESrc := src.RecoverESrcPath()

	srcRel, _ := filepath.Rel(deps.ProjectDir, recoverESrc)
	eCodeRel, _ := filepath.Rel(deps.ProjectDir, ecode)

	args := deps.C.E2Txt.ArgsTxt2E(ecode, recoverESrc)
	execE2TxtCmd(out, errorOccurs, eCodeRel, args, srcRel)
}

func convertE2Txt(out func(string), errorOccurs func(), src *sources.Source) {
	eSrcFile := src.AbsPath()
	ecode := src.ECodeDir()

	srcRel, _ := filepath.Rel(deps.ProjectDir, eSrcFile)
	eCodeRel, _ := filepath.Rel(deps.ProjectDir, ecode)

	args := deps.C.E2Txt.ArgsE2Txt(eSrcFile, ecode)
	execE2TxtCmd(out, errorOccurs, srcRel, args, eCodeRel)
}

func execE2TxtCmd(out func(string), errorOccurs func(), srcRel string, args []string, dstRel string) {
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
		out(color.Green.Sprintf("✔ [%s] -> [%s]", srcRel, dstRel))
	} else {
		errorOccurs()
		out(color.Red.Sprintf("❌ [%s] -> [%s] %v", srcRel, dstRel, errTips))
	}
}
