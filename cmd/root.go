package cmd

import (
	"errors"
	"fmt"
	"os"
	"path/filepath"

	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/sources"
	"github.com/spf13/cobra"
)

var rootCmd = &cobra.Command{
	Use:     "ebuild",
	Short:   "ebuild 是一个专注于易语言自动化构建的工具。",
	Long:    "使用Golang编写的、以 e2txt 和 ecl 作为基础工具的上层构建工具。",
	Version: "0.0.1",
	PersistentPreRunE: func(cmd *cobra.Command, args []string) error {
		if projectDir, err := filepath.Abs(deps.ProjectDir); err != nil {
			return errors.New("您输入的路径有误。")
		} else {
			deps.ProjectDir = projectDir
		}
		return nil
	},
}

func init() {
	initRootCmd()
	cobra.OnInitialize(func() {
		deps.Vp.SetConfigName("ebuild")
		deps.Vp.SetConfigType("yaml")
		deps.Vp.AddConfigPath(deps.ProjectDir)

		deps.Vp.SetDefault("e2txt.name-style", "中文")

		deps.PasswordResolver = &sources.FilePasswordResolver{
			File:       filepath.Join(deps.ProjectDir, "ebuild.pwd.yaml"),
			ProjectDir: deps.ProjectDir,
		}
	})
}

func initRootCmd() {
	curDir, _ := os.Getwd()
	rootCmd.PersistentFlags().StringVarP(&deps.ProjectDir, "project", "b", curDir, "指定工程的目录。默认为当前路径。")
	rootCmd.AddCommand(
		&initCmd,
		&infoCmd,
		&toolchainCommand,
		&e2txtCmd, &txt2eCmd,
		&buildCmd,
		&cleanCmd,
	)

	setMsgTemplate()
}

func setMsgTemplate() {
	rootCmd.SetUsageTemplate(`使用方法:{{if .Runnable}}
  {{.UseLine}}{{end}}{{if .HasAvailableSubCommands}}
  {{.CommandPath}} [command]{{end}}{{if gt (len .Aliases) 0}}

别名:
  {{.NameAndAliases}}{{end}}{{if .HasExample}}

示例:
{{.Example}}{{end}}{{if .HasAvailableSubCommands}}

可用命令:{{range .Commands}}{{if (or .IsAvailableCommand (eq .Name "help"))}}
  {{rpad .Name .NamePadding }} {{.Short}}{{end}}{{end}}{{end}}{{if .HasAvailableLocalFlags}}

参数:
{{.LocalFlags.FlagUsages | trimTrailingWhitespaces}}{{end}}{{if .HasAvailableInheritedFlags}}

通用参数:
{{.InheritedFlags.FlagUsages | trimTrailingWhitespaces}}{{end}}{{if .HasHelpSubCommands}}

其他帮助:{{range .Commands}}{{if .IsAdditionalHelpTopicCommand}}
  {{rpad .CommandPath .CommandPathPadding}} {{.Short}}{{end}}{{end}}{{end}}{{if .HasAvailableSubCommands}}

使用 "{{.CommandPath}} [命令] --help" 查看与命令更多的帮助信息.{{end}}
`)

	rootCmd.SetVersionTemplate(`{{with .Name}}{{printf "%s " .}}{{end}}{{printf "版本: %s" .Version}}`)
}

func Execute() {
	if err := rootCmd.Execute(); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}

func RootCommand() *cobra.Command {
	return rootCmd
}
