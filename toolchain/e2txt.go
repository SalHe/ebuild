package toolchain

import (
	"bufio"
	"golang.org/x/text/encoding/simplifiedchinese"
	"golang.org/x/text/transform"
	"io"
	"os/exec"
	"regexp"
	"strings"
)

// E2TxtCmd TODO 使用 Exec 实现
type E2TxtCmd struct {
	path     string
	args     []string
	OnLog    ReportFunc
	OnError  ReportFunc
	OnOutDir ReportFunc
	OnOver   func()
}

func reportNothing(s string) {}

func NewE2TxtCmd(path string, args ...string) *E2TxtCmd {
	return &E2TxtCmd{
		path:     path,
		args:     args,
		OnLog:    reportNothing,
		OnError:  reportNothing,
		OnOutDir: reportNothing,
		OnOver:   func() {},
	}
}

func (c *E2TxtCmd) Exec() {
	cmd := exec.Command(c.path, c.args...)
	stdout, _ := cmd.StdoutPipe()
	stderr, _ := cmd.StderrPipe()
	cmd.Start()

	overB := false
	go func() {
		cmd.Wait()
		c.OnOver()
		overB = true
	}()

	readToChan := func(pipe io.Reader) <-chan string {
		reader, incoming := bufio.NewReader(transform.NewReader(pipe, simplifiedchinese.GBK.NewDecoder())), make(chan string)
		go func() {
			for !overB {
				line, _, err := reader.ReadLine()
				if err != nil {
					continue
				}
				incoming <- string(line)
			}
		}()
		return incoming
	}
	stdoutIncoming := readToChan(stdout)
	stderrIncoming := readToChan(stderr)

	go func() {
		for !overB {
			select {
			case o := <-stdoutIncoming:
				o = strings.TrimSuffix(o, "\r")
				if strings.HasPrefix(o, "SUCC:") {
					c.OnOutDir(formatLog(o[5:]))
				} else if strings.HasPrefix(o, "ERROR:") {
					c.OnError(formatLog(o[6:]))
				} else if o != "LOG:" {
					c.OnLog(formatLog(o))
				}
			case e := <-stderrIncoming:
				e = strings.TrimPrefix(e, "ERROR:")
				e = strings.TrimSuffix(e, "\r")
				c.OnError(formatLog(e))
			}
		}
	}()
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
