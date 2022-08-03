package main

import (
	"encoding/json"
	"fmt"
	"io/fs"
	"os"
)

const versionTemplate = `package deps

// !!! 自动生成代码，请勿添加任何其他代码在此处

const Version = "%v"
`

func main() {
	packageJsonBytes, err := os.ReadFile("package.json")
	if err != nil {
		panic("不能正确读取package.json")
		return
	}

	var pkg struct {
		Version string `json:"version"`
	}

	err = json.Unmarshal(packageJsonBytes, &pkg)
	if err != nil {
		panic("不能正确解析")
		return
	}

	versionGo := fmt.Sprintf(versionTemplate, pkg.Version)
	err = os.WriteFile("./deps/version.go", []byte(versionGo), fs.ModePerm)
	if err != nil {
		panic("写出 ./deps/version.go 失败")
		return
	}

}
