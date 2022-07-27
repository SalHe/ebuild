package deps

import (
	"github.com/SalHe/ebuild/config"
	"github.com/SalHe/ebuild/sources"
	"github.com/spf13/viper"
)

var (
	C                config.Config
	Vp               = viper.New()
	ESrcs            []*sources.Source
	ProjectDir       string
	OutputDir        string
	PasswordResolver sources.PasswordResolver
)
