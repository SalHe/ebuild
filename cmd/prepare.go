package cmd

import (
	"errors"
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/sources"
	"github.com/SalHe/ebuild/utils"
	"github.com/mattn/go-zglob"
	"github.com/spf13/cobra"
	"path"
	"path/filepath"
)

func loadConfiguration(cmd *cobra.Command, args []string) error {
	_ = deps.Vp.ReadInConfig()

	configFileUsed := deps.Vp.ConfigFileUsed()
	if configFileUsed == "" {
		return errors.New("未找到配置文件。")
	}

	if err := deps.Vp.Unmarshal(&deps.C); err != nil {
		return errors.New("反序列化配置出错，请检查您的配置是否正确！" + err.Error())
	}

	deps.OutputDir = filepath.Join(deps.ProjectDir, "ebuild-out")

	fmt.Printf("已启用配置：%s\n", configFileUsed)
	loadSources()
	return nil
}

func loadSources() {
	eFiles := scanEFiles()
	loadSourceConfig(eFiles)
}

const (
	typeHandled = 0
	typeIgnore  = 0
	typeNormal  = 1
	typeNoBuild = 2
)

func loadSourceConfig(eFiles map[string]int) {
	var finalSources []*sources.Source

	for _, source := range deps.C.Targets {
		finalSrc := sources.FromYAML(source, deps.C.Build, deps.ProjectDir)
		finalSources = append(finalSources, finalSrc)
		eFiles[finalSrc.AbsPath()] = typeHandled
	}

	for p, keep := range eFiles {
		if //goland:noinspection GoBoolExpressions
		keep != typeHandled && keep != typeIgnore {
			noBuild := keep == typeNoBuild
			relPath, _ := filepath.Rel(deps.ProjectDir, p)
			finalSources = append(finalSources, sources.FromPath(relPath, deps.C.Build, deps.ProjectDir, noBuild))
		}
	}

	deps.ESrcs = finalSources
}

func scanEFiles() map[string]int {
	eSrc := make(map[string]int)
	searchAndSet := func(patterns []string, keep int) {
		for _, v := range patterns {
			searchPattern := path.Join(deps.ProjectDir, v)
			files, _ := zglob.Glob(searchPattern)
			for _, file := range files {
				file, _ = filepath.Abs(file)
				eSrc[file] = keep
			}
		}
	}
	searchAndSet(deps.C.Includes, typeNormal)
	searchAndSet(deps.C.ExcludeBuilds, typeNoBuild)
	searchAndSet(deps.C.Excludes, typeIgnore)

	return eSrc
}

func checkTool(path func() string, description string, inner func(cmd *cobra.Command, args []string) error) func(cmd *cobra.Command, args []string) error {
	return func(cmd *cobra.Command, args []string) error {
		if !utils.FileExists(path()) {
			return errors.New(fmt.Sprintf("未找到 %v", description))
		}
		return inner(cmd, args)
	}
}