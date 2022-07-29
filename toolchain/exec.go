package toolchain

import (
	"bufio"
	"golang.org/x/text/encoding/simplifiedchinese"
	"golang.org/x/text/transform"
	"io"
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
	}
}

func (c *Exec) Exec() {
	cmd := exec.Command(c.path, c.args...)
	stdout, _ := cmd.StdoutPipe()
	stderr, _ := cmd.StderrPipe()
	cmd.Start()

	overB := false
	go func() {
		cmd.Wait()
		c.onExit(cmd.ProcessState.ExitCode())
		c.onOver()
		overB = true

		c.over <- nil
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
