package cmd

import (
	"fmt"
	"github.com/SalHe/ebuild/deps"
	"github.com/SalHe/ebuild/sources"
	"github.com/mattn/go-zglob"
	"github.com/spf13/cobra"
	"os"
	"path"
	"path/filepath"
)

func loadConfiguration(cmd *cobra.Command, args []string) {
	_ = deps.Vp.ReadInConfig()

	configFileUsed := deps.Vp.ConfigFileUsed()
	if configFileUsed == "" {
		fmt.Println("未找到配置文件。")
		os.Exit(1)
	}

	if err := deps.Vp.Unmarshal(&deps.C); err != nil {
		fmt.Println("反序列化配置出错，请检查您的配置是否正确！")
		fmt.Println(err)
		os.Exit(1)
	}

	fmt.Printf("已启用配置：%s\n", configFileUsed)
	loadSources()
}

func loadSources() {
	eFiles := scanEFiles()
	loadSourceConfig(eFiles)

}

func loadSourceConfig(eFiles map[string]bool) {
	var finalSources []*sources.Source

	for _, source := range deps.C.Targets {
		finalSrc := sources.FromYAML(source, deps.BuildDir)
		if included, exist := eFiles[finalSrc.AbsPath()]; exist && included {
			finalSources = append(finalSources, finalSrc)
			eFiles[finalSrc.AbsPath()] = false
		}
	}

	for p, keep := range eFiles {
		if keep {
			relPath, _ := filepath.Rel(deps.BuildDir, p)
			finalSources = append(finalSources, sources.FromPath(relPath, deps.C.Build, deps.BuildDir))
		}
	}

	deps.ESrcs = finalSources
}

func scanEFiles() map[string]bool {
	eSrc := make(map[string]bool)
	searchAndSet := func(patterns []string, keep bool) {
		for _, v := range patterns {
			searchPattern := path.Join(deps.BuildDir, v)
			files, _ := zglob.Glob(searchPattern)
			for _, file := range files {
				eSrc[file] = keep
			}
		}
	}
	searchAndSet(deps.C.Includes, true)
	searchAndSet(deps.C.Excludes, false)

	return eSrc
}
