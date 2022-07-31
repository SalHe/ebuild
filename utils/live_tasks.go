package utils

type LiveTasks struct {
	liveLines *LiveLines
	executor  *TasksExecutor

	over  bool
	allOk bool
}

type UpdateDisplayFunc func(display string)
type LiveTasksHandlerFunc func(id int, te *TasksExecutor, update UpdateDisplayFunc) error

func NewLiveTasks(tasks int, concurrency int) *LiveTasks {
	return &LiveTasks{
		liveLines: NewLiveLines(tasks),
		executor:  NewTasksExecutor(tasks, concurrency),
		allOk:     true,
		over:      false,
	}
}

func (l *LiveTasks) Header(header func(over bool, allOk bool) string) {
	l.liveLines.Header(func() string {
		return header(l.over, l.allOk)
	})
}

func (l *LiveTasks) OnPreRun(handler LiveTasksHandlerFunc) {
	l.executor.OnPreRun = func(id int, te *TasksExecutor) error {
		return handler(id, te, l.updateDisplay(id))
	}
}

func (l *LiveTasks) updateDisplay(id int) UpdateDisplayFunc {
	return func(display string) {
		l.liveLines.Update(id, display)
	}
}

func (l *LiveTasks) OnRun(handler LiveTasksHandlerFunc) {
	l.executor.OnRun = func(id int, te *TasksExecutor) error {
		err := handler(id, te, l.updateDisplay(id))
		if err != nil {
			l.allOk = false
		}
		return err
	}
}

func (l *LiveTasks) Start() {
	l.liveLines.Start()
	l.executor.Start()
}

func (l *LiveTasks) Wait() {
	l.executor.Wait()
	l.over = true
	l.liveLines.Stop()
}

func (l *LiveTasks) Over() bool {
	return l.over
}

func (l *LiveTasks) AllOk() bool {
	return l.allOk
}

func (l *LiveTasks) StartAndWait() {
	l.Start()
	l.Wait()
}
