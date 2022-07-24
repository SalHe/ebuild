package deps

import (
	"github.com/SalHe/ebuild/config"
	"github.com/SalHe/ebuild/sources"
	"github.com/spf13/viper"
)

var (
	C                config.Config
	Vp               *viper.Viper = viper.New()
	ESrcs            []string
	BuildDir         string
	PasswordResolver sources.PasswordResolver
)
