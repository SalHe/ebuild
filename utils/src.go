package utils

import "path/filepath"

func ECodeDir(source string) string {
	dir, name, _ := FilePathElements(source)
	target := dir + string(filepath.Separator) + name + ".ecode"
	return target
}
