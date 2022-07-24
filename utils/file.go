package utils

import (
	"errors"
	"io/fs"
	"os"
)

func FileExists(path string) bool {
	_, err := os.Stat(path)
	return !errors.Is(err, fs.ErrNotExist)
}
