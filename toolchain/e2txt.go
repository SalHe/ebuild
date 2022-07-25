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
				o = strings.TrimSuffix(o, "\r")
				if strings.HasPrefix(o, "SUCC:") {
					outDirChan <- formatLog(o[5:])
				} else if strings.HasPrefix(o, "ERROR:") {
					errorChan <- formatLog(o[6:])
				} else if o != "LOG:" {
					logChan <- formatLog(o)
				}
			case e := <-stderrIncoming:
				e = strings.TrimPrefix(e, "ERROR:")
				e = strings.TrimSuffix(e, "\r")
				errorChan <- formatLog(e)
			}
		}
	}()

	return
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
