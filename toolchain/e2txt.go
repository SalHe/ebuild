package toolchain

import (
	"regexp"
	"strings"
)

type E2TxtCmd struct {
	exec     *Exec
	onLog    ReportFunc
	onError  ReportFunc
	onOutDir ReportFunc
}

func reportNothing(s string) {}

func NewE2TxtCmd(path string, args ...string) *E2TxtCmd {
	return &E2TxtCmd{
		exec:     NewExec(path, args...),
		onLog:    reportNothing,
		onError:  reportNothing,
		onOutDir: reportNothing,
	}
}

func (c *E2TxtCmd) OnLog(onLog ReportFunc) {
	c.onLog = onLog
	c.exec.OnStdout(func(log string) {
		log = strings.TrimSuffix(log, "\r")
		if strings.HasPrefix(log, "SUCC:") {
			c.onOutDir(formatLog(log[5:]))
		} else if strings.HasPrefix(log, "ERROR:") {
			c.onError(formatLog(log[6:]))
		} else if log != "LOG:" {
			c.onLog(formatLog(log))
		}
	})
}

func (c *E2TxtCmd) OnError(onError ReportFunc) {
	c.onError = onError
	c.exec.OnStderr(func(e string) {
		e = strings.TrimPrefix(e, "ERROR:")
		e = strings.TrimSuffix(e, "\r")
		c.onError(formatLog(e))
	})
}

func (c *E2TxtCmd) OnOutDir(onOutDir ReportFunc) {
	c.onOutDir = onOutDir
}

func (c *E2TxtCmd) OnExit(onExit ExitFunc) {
	c.exec.OnExit(onExit)
}

func (c *E2TxtCmd) Exec() {
	c.exec.Exec()
}

var spaces = regexp.MustCompile("\\s{2,}")

func formatLog(o string) string {
	return string(spaces.ReplaceAll([]byte(takeOffTime(o)), []byte(" "))) // 把两个或两个以上的空白字符替换成一个
}

func takeOffTime(o string) string {
	if len(o) >= 11 && o[len("2022-07-25 09:")-1] == ':' {
		return o[len("2022-07-25 09:36:24 "):]
	}
	return o
}
