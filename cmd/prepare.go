package cmd

import (
	"errors"
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/sources"
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

func loadSourceConfig(eFiles map[string]bool) {
	var finalSources []*sources.Source

	for _, source := range deps.C.Targets {
		finalSrc := sources.FromYAML(source, deps.C.Build, deps.ProjectDir)
		finalSources = append(finalSources, finalSrc)
		eFiles[finalSrc.AbsPath()] = false
	}

	for p, keep := range eFiles {
		if keep {
			relPath, _ := filepath.Rel(deps.ProjectDir, p)
			finalSources = append(finalSources, sources.FromPath(relPath, deps.C.Build, deps.ProjectDir))
		}
	}

	deps.ESrcs = finalSources
}

func scanEFiles() map[string]bool {
	eSrc := make(map[string]bool)
	searchAndSet := func(patterns []string, keep bool) {
		for _, v := range patterns {
			searchPattern := path.Join(deps.ProjectDir, v)
			files, _ := zglob.Glob(searchPattern)
			for _, file := range files {
				file, _ = filepath.Abs(file)
				eSrc[file] = keep
			}
		}
	}
	searchAndSet(deps.C.Includes, true)
	searchAndSet(deps.C.Excludes, false)

	return eSrc
}
