package cmd

import (
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/toolchain"
	"github.com/SalHe/ebuild/utils"
	"github.com/gookit/color"
	"github.com/gosuri/uilive"
	"github.com/spf13/cobra"
	"path/filepath"
	"strings"
	"sync"
	"time"
)

const concurrencyCount = 5

var e2txtCmd = cobra.Command{
	Use:    "e2txt",
	Short:  "将项目中包含的源文件按照约定配置转换成文本式的代码。",
	PreRun: loadConfiguration,
	Run:    runE2Txt,
}

func runE2Txt(cmd *cobra.Command, args []string) {
	// TODO 代码等待整理优化

	live := uilive.New()
	live.Start()
	concurrency := make(chan interface{}, concurrencyCount)
	for i := 0; i < concurrencyCount; i++ {
		concurrency <- nil
	}
	wg := &sync.WaitGroup{}
	prints := make([]string, len(deps.ESrcs))
	over := false
	allOk := true

	for i, src := range deps.ESrcs {
		wg.Add(1)
		j := i
		update := func(o string) {
			prints[j] = o
		}
		errorOccurs := func() {
			allOk = false
		}
		go e2txtSrc(update, errorOccurs, src, concurrency, wg)
	}

	printProgress := func() {
		var toPrint string
		if over {
			if allOk {
				toPrint = "转换已完成！\n\n"

			} else {
				toPrint = color.Red.Render("转换结束，部分源码转换失败，请注意查看！\n\n")
			}
		} else {
			toPrint = "正在为您转换源文件，请耐心等候：\n\n"
		}
		for _, s := range prints {
			toPrint += s
		}
		fmt.Fprintln(live.Newline(), toPrint)
		live.Flush()
	}

	go func() {
		for !over {
			select {
			case <-time.After(live.RefreshInterval):
				printProgress()
			}
		}
	}()
	wg.Wait()
	over = true

	printProgress()
	live.Stop()
}

func eCodeDir(source string) string {
	dir, name, _ := utils.FilePathElements(source)
	target := dir + string(filepath.Separator) + name + ".ecode"
	return target
}

func e2txtSrc(out func(string), errorOccurs func(), src string, concurrency chan interface{}, wg *sync.WaitGroup) {
	ecode := eCodeDir(src)
	srcRel, _ := filepath.Rel(deps.BuildDir, src)
	eCodeRel, _ := filepath.Rel(deps.BuildDir, ecode)

	out(fmt.Sprintf("[等待中][%s]\n", srcRel))
	<-concurrency
	defer func() {
		wg.Done()
		concurrency <- nil
	}()

	args := deps.C.E2Txt.ArgsE2Txt(src, ecode)

	wrong := false
	errTips := ""
	cmdOver := make(chan interface{})

	out(fmt.Sprintf("[开始转换][%s]: %s\n", srcRel, color.Gray.Render(toolchain.E2Txt()+" "+strings.Join(args, " "))))

	cmd := toolchain.NewE2TxtCmd(toolchain.E2Txt(), args...)
	cmd.OnLog = func(log string) { out(fmt.Sprintf("[转换中][%s]: %s\n", srcRel, log)) }
	cmd.OnError = func(err string) {
		wrong = true
		errTips += "\n\t" + err // 错误提示可以叠加，方便查看过程中出现的所有错误
		out(fmt.Sprintf("[转换出错][%s]: 转换出错 %v\n", srcRel, err))
	}
	cmd.OnOutDir = func(outDir string) { out(fmt.Sprintf("[转换完成][%s]: 已保存到%s\n", srcRel, outDir)) }
	cmd.OnOver = func() { cmdOver <- nil }
	cmd.Exec()

	<-cmdOver

	if !wrong {
		out(fmt.Sprintf(color.Green.Sprintf("✔ [%s] -> [%s]\n", srcRel, eCodeRel)))
	} else {
		errorOccurs()
		out(fmt.Sprintf(color.Red.Sprintf("❌ [%s] -> [%s] %v\n", srcRel, eCodeRel, errTips)))
	}
}
