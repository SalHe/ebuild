package toolchain

import (
	"bufio"
	"golang.org/x/text/encoding/simplifiedchinese"
	"golang.org/x/text/transform"
	"io"
	"os/exec"
	"strings"
)

func ExecE2Txt(path string, args []string) (over <-chan interface{}, log <-chan string, outDir <-chan string, error <-chan string) {
	cmd := exec.Command(path, args...)
	stdout, _ := cmd.StdoutPipe()
	stderr, _ := cmd.StderrPipe()
	cmd.Start()

	overChan := make(chan interface{}, 3)
	over = overChan
	overB := false
	go func() {
		cmd.Wait()
		overChan <- nil
		overB = true
	}()

	logChan := make(chan string)
	log = logChan
	errorChan := make(chan string)
	error = errorChan
	outDirChan := make(chan string)
	outDir = outDirChan

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
				o = o[:len(o)-1] // \r
				if strings.HasPrefix(o, "SUCC:") {
					outDirChan <- o[5:]
				} else if o != "LOG:" {
					logChan <- o
				}
			case e := <-stderrIncoming:
				errorChan <- e[6:]
			}
		}
	}()

	return
}
