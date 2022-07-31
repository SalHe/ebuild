package utils

import "sync"

type TaskHandlerFunc func(id int, te *TasksExecutor) error

type TasksExecutor struct {
	tasks int

	concurrency chan interface{}
	wg          *sync.WaitGroup

	started bool

	OnPreRun TaskHandlerFunc
	OnRun    TaskHandlerFunc
}

func NewTasksExecutor(tasks int, concurrency int) *TasksExecutor {
	return &TasksExecutor{
		tasks:       tasks,
		concurrency: make(chan interface{}, concurrency),
		wg:          &sync.WaitGroup{},
	}
}

func (te *TasksExecutor) Start() {
	if te.started {
		panic("任务已启动")
	}

	for i := 0; i < cap(te.concurrency); i++ {
		te.concurrency <- nil
	}

	for t := 0; t < te.tasks; t++ {
		t := t
		te.wg.Add(1)
		go func() {
			te.OnPreRun(t, te)
			<-te.concurrency

			te.OnRun(t, te)
			te.concurrency <- nil
			te.wg.Done()
		}()
	}
}

func (te *TasksExecutor) Wait() {
	te.wg.Wait()
}
