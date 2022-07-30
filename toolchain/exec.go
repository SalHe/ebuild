package toolchain

import (
	"bufio"
	"fmt"
	"golang.org/x/text/encoding/simplifiedchinese"
	"golang.org/x/text/transform"
	"io"
	"os"
	"os/exec"
)

type ReportFunc func(string)
type ExitFunc func(code int)

type Exec struct {
	path    string
	args    []string
	over    chan interface{}
	onLog   ReportFunc
	onError ReportFunc
	onExit  ExitFunc
	onOver  func()

	gbk bool

	cmd *exec.Cmd
}

func (c *Exec) SetGbk(gbk bool) {
	c.gbk = gbk
}

func (c *Exec) OnExit(onExit ExitFunc) {
	c.onExit = onExit
}

func (c *Exec) OnLog(onLog ReportFunc) {
	c.onLog = onLog
}

func (c *Exec) OnError(onError ReportFunc) {
	c.onError = onError
}

func (c *Exec) OnOver(onOver func()) {
	c.onOver = onOver
}

func NewExec(path string, args ...string) *Exec {
	return &Exec{
		path:    path,
		args:    args,
		over:    make(chan interface{}),
		onLog:   reportNothing,
		onError: reportNothing,
		onExit:  func(code int) {},
		onOver:  func() {},
		cmd:     exec.Command(path, args...),

		gbk: true,
	}
}

func (c *Exec) Exec() {
	c.cmd.Env = append(os.Environ(), c.cmd.Env...)
	stdout, _ := c.cmd.StdoutPipe()
	stderr, _ := c.cmd.StderrPipe()
	c.cmd.Start()

	overB := false
	go func() {
		c.cmd.Wait()
		c.onExit(c.cmd.ProcessState.ExitCode())
		c.onOver()
		overB = true

		c.over <- nil
	}()

	readToChan := func(pipe io.Reader) <-chan string {
		var newReader io.Reader
		if c.gbk {
			newReader = transform.NewReader(pipe, simplifiedchinese.GBK.NewDecoder())
		} else {
			newReader = pipe
		}

		reader, incoming := bufio.NewReader(newReader), make(chan string)
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
				c.onLog(o)
			case e := <-stderrIncoming:
				c.onError(e)
			}
		}
	}()
}

func (c *Exec) Wait() {
	<-c.over
}

func (c *Exec) LoadEnv(env map[string]string) {
	for key, value := range env {
		c.cmd.Env = append(c.cmd.Env, fmt.Sprintf("%v=%v", key, value))
	}
}
