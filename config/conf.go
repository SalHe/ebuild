package config

type Config struct {
	Project  *Project `yaml:"project"`
	Includes []string `yaml:"includes"`
	Excludes []string `yaml:"excludes"`
}

type Project struct {
	Name       string    `yaml:"name"`
	Version    string    `yaml:"version"`
	Author     string    `yaml:"author"`
	Repository string    `yaml:"repository"`
	Homepage   string    `yaml:"homepage"`
	Source     []*Source `yaml:"sources"`
}
