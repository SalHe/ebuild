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

var e2txtCmd = cobra.Command{
	Use:    "e2txt",
	Short:  "将项目中包含的源文件按照约定配置转换成文本式的代码。",
	PreRun: loadConfiguration,
	Run: func(cmd *cobra.Command, args []string) {
		for _, src := range deps.ESrcs {
			ecode := eCodeDir(src)
			srcRel, _ := filepath.Rel(deps.BuildDir, src)
			eCodeRel, _ := filepath.Rel(deps.BuildDir, ecode)

			args := deps.C.E2Txt.ArgsE2Txt(src, ecode)

			over, logChan, outDirChan, errChan := toolchain.ExecE2Txt(toolchain.E2Txt(), args)
			finished, wrong := false, false

			fmt.Printf("正在转换：%s\n", srcRel)
			color.Grayln(toolchain.E2Txt(), strings.Join(args, " "))
			for !finished {
				select {
				case log := <-logChan:
					fmt.Println(log)
				case outDir := <-outDirChan:
					color.Greenf("已保存到：%v\n", outDir)
				case err := <-errChan:
					wrong = true
					color.Redln(err)
				case <-over:
					finished = true
					break
				}
			}

			if !wrong {
				color.Greenf("✔ %s -> %s\n", srcRel, eCodeRel)
			} else {
				color.Redf("❌ %s -> %s (%v)\n", srcRel, eCodeRel)
			}
		}
	},
}

func eCodeDir(source string) string {
	dir, name, _ := utils.FilePathElements(source)
	target := dir + string(filepath.Separator) + name + ".ecode"
	return target
}
