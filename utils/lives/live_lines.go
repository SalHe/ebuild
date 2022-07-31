package lives

import (
	"fmt"
	"github.com/gosuri/uilive"
	"strings"
	"sync"
)

type LiveLines struct {
	live   *uilive.Writer
	lines  []string
	done   chan interface{}
	header func() string
	update chan bool
	mutex  *sync.Mutex
}

func NewLiveLines(lines int) *LiveLines {
	return &LiveLines{
		live:   uilive.New(),
		lines:  make([]string, lines),
		done:   make(chan interface{}),
		update: make(chan bool),
		mutex:  &sync.Mutex{},
	}
}

func (l *LiveLines) Listen() {
	for {
		select {
		case <-l.update:
			l.print()
			l.live.Flush()
		case <-l.done:
			goto done
		}
	}
done:
	close(l.done)
}

func (l *LiveLines) print() (int, error) {
	return fmt.Fprintln(l.live, l.printContent())
}

func (l *LiveLines) printContent() string {
	return l.header() + strings.Join(l.lines, "\n")
}

func (l *LiveLines) Header(header func() string) {
	l.header = header
}

func (l *LiveLines) Start() {
	l.live.Start()
	go l.Listen()
}

func (l *LiveLines) Stop() {
	l.done <- nil
	l.print()
	l.live.Stop()
}

func (l *LiveLines) Update(lineNo int, content string) {
	l.mutex.Lock()
	defer l.mutex.Unlock()

	l.lines[lineNo] = content
	l.update <- true
}
