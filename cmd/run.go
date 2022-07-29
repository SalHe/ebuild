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
	"io/ioutil"
	"os"
	"path/filepath"
)

var runCmd = cobra.Command{
	Use:   "run 脚本名或易语言源文件",
	Short: "执行脚本或易语言源文件",
	Long: `当第一个参数对应了配置文件中某一脚本名字时，将执行脚本对应的内容；
当第一个参数对应了某一个源码的文件名时，将编译该易语言源文件并执行(要求源文件必须是Windows可执行程序或Windows控制台程序，由于需要编译，建议源文件尽可能小)。

由于执行的对象可能是脚本或者易语言源文件，所以建议您为脚本命名的时候不要与源文件重名。
此外，目前ebuild不会保证到底先执行哪种情况，所以更加建议您不要重名。

当您执行易语言源文件时，请您自行确保源文件的安全性。
`,
	Args:    cobra.ExactArgs(1),
	PreRunE: loadConfiguration,
	RunE: func(cmd *cobra.Command, args []string) error {
		scriptOrFile := args[0]
		if path, yes := isSourceFile(scriptOrFile); yes {
			return execESourceFile(path)
		} else {
			// TODO 实现执行脚本
			fmt.Println("执行脚本：" + scriptOrFile + " （等待实现）")
			return nil
		}
	},
}

var verbose = false

func init() {
	runCmd.Flags().BoolVar(&verbose, "verbose", verbose, "是否显示冗余信息。目前冗余信息主要是编译过程的日志。")
}

func isSourceFile(path string) (absPath string, ok bool) {
	if filepath.Ext(path) != ".e" {
		return "", false
	}
	if !filepath.IsAbs(path) {
		path, _ = filepath.Abs(path)
	}
	if utils.FileExists(path) {
		return path, true
	}
	return "", false
}

func execESourceFile(file string) error {
	exeTemp, err := ioutil.TempFile("", "*.ebuild-run.exe")
	if err != nil {
		return errors.New("创建临时文件失败")
	}
	exeTemp.Close()

	fmt.Println()
	color.Redf("即将执行 [%v] (请您自行确保源文件的安全性)\n", file)

	defer os.Remove(exeTemp.Name())

	outputDir := ""

	source := sources.FromPath(file, nil, deps.ProjectDir, true)
	source.Output = exeTemp.Name()
	targetType := source.TargetType()
	if source.SourceType() != sources.ESourceSrc || !targetType.IsWindowsExecutable() {
		return errors.New("源文件必须编译为Windows可执行程序")
	}
	compileOk := compileESource(source, verbose, outputDir)

	if !compileOk {
		return errors.New("编译源文件失败")
	}

	fmt.Println()
	color.Yellowf("编译成功，开始执行 [%v]\n", source.OutputPath(outputDir))
	fmt.Println()

	exitCode := 0
	exec := toolchain.NewExec(source.OutputPath(outputDir))
	exec.OnLog(func(log string) {
		fmt.Println(log)
	})
	exec.OnError(func(err string) {
		color.Redln(err)
	})
	exec.OnExit(func(code int) {
		exitCode = code
	})
	exec.Exec()
	exec.Wait()

	if exitCode != 0 {
		return errors.New(fmt.Sprintf("退出代码：%v", exitCode))
	}
	return nil
}

func compileESource(source *sources.Source, verbose bool, outputDir string) bool {
	color.Yellowf("正在编译 [%v] -> [%v]\n", source.Source, source.OutputPath(outputDir))

	args := source.CompileArgs(outputDir, "")

	compileOk := true

	eclCmd := toolchain.NewEclCmd(toolchain.Ecl(), args...)
	eclCmd.OnError(func(err string) {
		compileOk = false
		color.Redln(err)
	})
	if verbose {
		eclCmd.OnLog(func(log string) {
			color.Grayln(log)
		})
	}
	eclCmd.Exec()
	eclCmd.Wait()
	return compileOk
}
