package sources

import (
	"os"
	"sync"

	"gopkg.in/yaml.v3"
)

type PasswordResolver interface {
	Resolve(source string) string
}

type FilePasswordResolver struct {
	File   string
	loaded bool
	pwd    map[string]string

	mutex sync.Mutex
}

func (f *FilePasswordResolver) Resolve(source string) string {
	f.mutex.Lock()
	defer f.mutex.Unlock()

	if !f.loaded {
		if bytes, err := os.ReadFile(f.File); err == nil {
			yaml.Unmarshal(bytes, &f.pwd)
		}
		f.loaded = true
	}
	return f.pwd[source]
}
