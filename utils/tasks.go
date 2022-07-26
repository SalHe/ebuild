package utils

import "sync"

type TasksExecutor struct {
	tasks int

	concurrency chan interface{}
	wg          *sync.WaitGroup

	started bool

	OnPreExec func(id int, te *TasksExecutor)
	OnExec    func(id int, te *TasksExecutor)
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
		go func() {
			te.wg.Add(1)
			te.OnPreExec(t, te)
			<-te.concurrency

			te.OnExec(t, te)
			te.concurrency <- nil
			te.wg.Done()
		}()
	}
}

func (te *TasksExecutor) Wait() {
	te.wg.Wait()
}
