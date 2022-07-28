package sources

import (
	"os"
	"path/filepath"
	"sync"

	"gopkg.in/yaml.v3"
)

type PasswordResolver interface {
	Resolve(source string) string
}

type FilePasswordResolver struct {
	File       string
	ProjectDir string
	loaded     bool
	pwd        map[string]string

	mutex sync.Mutex
}

func (f *FilePasswordResolver) Resolve(source string) string {
	f.mutex.Lock()
	defer f.mutex.Unlock()

	if !f.loaded {
		var pwd map[string]string
		if bytes, err := os.ReadFile(f.File); err == nil {
			yaml.Unmarshal(bytes, &pwd)
		}

		f.pwd = make(map[string]string)
		for file, p := range pwd {
			f.pwd[filepath.Join(f.ProjectDir, file)] = p
		}

		f.loaded = true
	}
	return f.pwd[filepath.Join(f.ProjectDir, source)]
}
