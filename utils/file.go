package utils

import (
	"errors"
	"io/fs"
	"os"
	"path/filepath"
	"strings"
)

func FileExists(path string) bool {
	_, err := os.Stat(path)
	return !errors.Is(err, fs.ErrNotExist)
}

func FilePathElements(path string) (dir, name, ext string) {
	dir = filepath.Dir(path)
	ext = filepath.Ext(path)
	name = strings.TrimSuffix(filepath.Base(path), ext)
	return
}
