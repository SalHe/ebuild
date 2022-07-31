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

type LogReader func(reader *bufio.Reader) (bytes []byte, err error)
type ReportFunc func(string)
type ExitFunc func(code int)

var (
	ReadLogByLine LogReader = func(reader *bufio.Reader) (bytes []byte, err error) {
		bytes, _, err = reader.ReadLine()
		return
	}
	ReadLogRealTime LogReader = func(reader *bufio.Reader) (bytes []byte, err error) {
		bytes = make([]byte, 1024)
		size := 0
		size, err = reader.Read(bytes)
		bytes = bytes[:size]
		return
	}
)

type Exec struct {
	path     string
	args     []string
	over     chan interface{}
	onStdout ReportFunc
	onStderr ReportFunc
	onExit   ExitFunc

	gbk bool

	cmd       *exec.Cmd
	logReader LogReader
}

func (c *Exec) SetLogReader(logReader LogReader) {
	c.logReader = logReader
}

func (c *Exec) SetGbk(gbk bool) {
	c.gbk = gbk
}

func (c *Exec) OnExit(onExit ExitFunc) {
	c.onExit = onExit
}

func (c *Exec) OnStdout(onLog ReportFunc) {
	c.onStdout = onLog
}

func (c *Exec) OnStderr(onError ReportFunc) {
	c.onStderr = onError
}

func NewExec(path string, args ...string) *Exec {
	return &Exec{
		path:     path,
		args:     args,
		over:     make(chan interface{}),
		onStdout: reportNothing,
		onStderr: reportNothing,
		onExit:   func(code int) {},

		cmd:       exec.Command(path, args...),
		logReader: ReadLogByLine,

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
				bytes, err := c.logReader(reader)
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
				c.onStdout(o)
			case e := <-stderrIncoming:
				c.onStderr(e)
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
