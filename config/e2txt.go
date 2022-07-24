package config

type nameStyle string
type e2txtMode string

const (
	StyleEg = "英文"
	StyleCh = "中文"
)

type E2Txt struct {
	Style     string `mapstructure:"name-style"`
	GenerateE bool   `mapstructure:"generate-e"`
}

func (et *E2Txt) args(from string, to string, mode e2txtMode) (args []string) {
	args = append(args, "-src", from, "-dst", to, "-mode", string(mode))
	args = append(args, et.generalArgs()...)
	return
}

func (et *E2Txt) ArgsE2Txt(source string, ecode string) (args []string) {
	return et.args(source, ecode, "e2t")
}

func (et *E2Txt) ArgsTxt2E(ecode string, source string) (args []string) {
	return et.args(ecode, source, "t2e")
}

func (et *E2Txt) generalArgs() (args []string) {
	switch et.Style {
	case StyleEg:
		args = append(args, "-ns", "1")
	case StyleCh:
		args = append(args, "-ns", "2")
	}

	if et.GenerateE {
		args = append(args, "-e")
	}

	args = append(args, "-log")
	args = append(args, "-enc", "UTF-8")

	return
}
