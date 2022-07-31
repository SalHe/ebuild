package toolchain

import (
	"regexp"
	"strconv"
)

type EclError int

const EclErrorSuccess EclError = 1
const (
	EclErrorOk EclError = -iota
	EclErrorUnknown
	EclErrorParam
	EclErrorFileNotFound
	EclErrorFileInvalid
	EclErrorCompile
	EclErrorInvalidCompileType
	EclErrorECannotStart
	EclErrorCanNotGetMenu
	EclErrorShutdown
	EclErrorStatic
	EclErrorMakeLinkIni
	EclErrorBmInfo
	EclErrorBmCompile
	EclErrorPassword
	EclErrorEC
	EclErrorELib
	EclErrorStartTimeout
	EclErrorCompileTimeout
	EclErrorNotSupportEPkg
)

var eclErrorTips = map[EclError]string{
	EclErrorSuccess:            "处理成功",
	EclErrorOk:                 "编译成功", // 未发生错误
	EclErrorUnknown:            "未定义类型的错误",
	EclErrorParam:              "命令行有错误",
	EclErrorFileNotFound:       "找不到文件",
	EclErrorFileInvalid:        "文件无效",
	EclErrorCompile:            "编译失败",
	EclErrorInvalidCompileType: "不支持的编译类型",
	EclErrorECannotStart:       "无法识别或无法运行的易语言程序",
	EclErrorCanNotGetMenu:      "无法获取易语言菜单",
	EclErrorShutdown:           "易语言意外结束",
	EclErrorStatic:             "静态编译失败",
	EclErrorMakeLinkIni:        "生成link.ini文件过程中出错",
	EclErrorBmInfo:             "老版黑月的相关数据无法定位",
	EclErrorBmCompile:          "黑月编译失败",
	EclErrorPassword:           "源码密码不正确",
	EclErrorEC:                 "缺乏易模块",
	EclErrorELib:               "缺少支持库",
	EclErrorStartTimeout:       "启动易语言超时",
	EclErrorCompileTimeout:     "编译超时",
	EclErrorNotSupportEPkg:     "不支持易包编译",
}

func (e EclError) IsOk() bool {
	return e == EclErrorSuccess || e == EclErrorOk
}

func EclErrorTips(code EclError) string {
	return eclErrorTips[code]
}

type EclCmd struct {
	exec   *Exec
	onExit ExitFunc
}

func NewEclCmd(path string, args ...string) *EclCmd {
	return &EclCmd{
		exec:   NewExec(path, args...),
		onExit: func(code int) {},
	}
}

var eclMatchError = regexp.MustCompile("\\(错误:(-\\d+)\\)(.+)")

func (c *EclCmd) OnLog(onLog ReportFunc) {
	c.exec.OnStdout(func(s string) {
		if eclMatchError.MatchString(s) {
			sm := eclMatchError.FindStringSubmatch(s)
			c.exec.onStderr(sm[2])
			code, _ := strconv.Atoi(sm[1])
			// c.exec.onExit(code)
			c.onExit(code)
		} else {
			onLog(s)
		}
	})
}

func (c *EclCmd) OnError(onError ReportFunc) {
	c.exec.OnStderr(onError)
}

func (c *EclCmd) OnExit(onExit ExitFunc) {
	c.onExit = onExit
	c.exec.OnExit(onExit)
}

func (c *EclCmd) Exec() {
	c.exec.Exec()
}

func (c *EclCmd) Wait() {
	c.exec.Wait()
}
