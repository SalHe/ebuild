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

	cmd        *exec.Cmd
	ReadByLine bool
}

func (c *Exec) SetGbk(gbk bool) {
	c.gbk = gbk
}

func (c *Exec) OnExit(onExit ExitFunc) {
	c.onExit = onExit
}

// OnLog 设置接收来自标准输出的内容回调。
// 当设置回调后如果 ReadByLine 为 true，回调接收的一行输出内容一定使没有行结束符的。
// 然而，当 ReadByLine 为 false 时，回调接收的一定是原始的内容。OnError 类似。
func (c *Exec) OnLog(onLog ReportFunc) {
	c.onLog = onLog
}

// OnError 设置接收来自标准错误输出的内容回调。
// 行为与 OnLog 类似。
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

		cmd:        exec.Command(path, args...),
		ReadByLine: true,

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
				var bytes []byte
				var err error = nil
				if c.ReadByLine {
					bytes, _, err = reader.ReadLine()
				} else {
					bytes = make([]byte, 1024)
					size := 0
					size, err = reader.Read(bytes)
					bytes = bytes[:size]
				}
				if err != nil {
					continue
				}
				incoming <- string(bytes)
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

func (c *Exec) ForwardStdin() {
	c.cmd.Stdin = os.Stdin
}
