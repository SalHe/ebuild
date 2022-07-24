package config

type Config struct {
	Project  *Project `mapstructure:"project"`
	Includes []string `mapstructure:"includes"`
	Excludes []string `mapstructure:"excludes"`
	E2Txt    *E2Txt   `mapstructure:"e2txt"`
}

type Project struct {
	Name        string    `mapstructure:"name"`
	Description string    `mapstructure:"description"`
	Version     string    `mapstructure:"version"`
	Author      string    `mapstructure:"author"`
	Repository  string    `mapstructure:"repository"`
	Homepage    string    `mapstructure:"homepage"`
	Source      []*Source `mapstructure:"sources"`
}
