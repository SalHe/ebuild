package cmd

import (
	"fmt"
	"os"
	"path"
	"path/filepath"

	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/sources"
	"github.com/mattn/go-zglob"
	"github.com/spf13/cobra"
)

var rootCmd = &cobra.Command{
	Use:     "ebuild",
	Short:   "ebuild 是一个专注于易语言自动化构建的工具。",
	Long:    "使用Golang编写的、以 e2txt 和 ecl 作为基础工具的上层构建工具。",
	Version: "0.0.1",
	PersistentPreRun: func(cmd *cobra.Command, args []string) {
		if buildDir, err := filepath.Abs(deps.BuildDir); err != nil {
			fmt.Println("您输入的路径有误。")
			os.Exit(1)
		} else {
			deps.BuildDir = buildDir
		}
	},
}

func init() {
	initRootCmd()
	cobra.OnInitialize(func() {
		deps.Vp.SetConfigName("ebuild")
		deps.Vp.SetConfigType("yaml")
		deps.Vp.AddConfigPath(deps.BuildDir)

		deps.PasswordResolver = &sources.FilePasswordResolver{
			File: filepath.Join(deps.BuildDir, "ebuild.pwd.yaml"),
		}
	})
}

func loadConfiguration(cmd *cobra.Command, args []string) {
	_ = deps.Vp.ReadInConfig()

	configFileUsed := deps.Vp.ConfigFileUsed()
	if configFileUsed != "" {
		fmt.Printf("已启用配置：%s\n", configFileUsed)
		loadSources(configFileUsed)
	} else {
		fmt.Println("未找到配置文件。")
		os.Exit(1)
	}
}

func initRootCmd() {
	curDir, _ := os.Getwd()
	rootCmd.PersistentFlags().StringVarP(&deps.BuildDir, "build", "b", curDir, "指定构建的目录。")
	rootCmd.AddCommand(&initCmd, &listCmd, &toolchainCommand)
}

func loadSources(configFileUsed string) {
	if err := deps.Vp.Unmarshal(&deps.C); err != nil {
		fmt.Println("反序列化配置出错，请检查您的配置是否正确！")
		fmt.Println(err)
		os.Exit(1)
	}

	eSrc := make(map[string]bool)
	searchAndSet := func(patterns []string, keep bool) {
		for _, v := range patterns {
			searchPattern := path.Join(filepath.Dir(configFileUsed), v)
			files, _ := zglob.Glob(searchPattern)
			for _, file := range files {
				eSrc[file] = keep
			}
		}
	}
	searchAndSet(deps.C.Includes, true)
	searchAndSet(deps.C.Excludes, false)

	for file, included := range eSrc {
		if included {
			deps.ESrcs = append(deps.ESrcs, file)
		}
	}
}

func Execute() {
	if err := rootCmd.Execute(); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}
