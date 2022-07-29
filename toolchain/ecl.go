package toolchain

import (
	"regexp"
	"strconv"
)

const EclErrorSuccess = 1
const (
	EclErrorOk = -iota
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

var eclErrorTips = map[int]string{
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
	EclErrorBmInfo:             "生成link.ini文件过程中出错",
	EclErrorBmCompile:          "老版黑月的相关数据无法定位",
	EclErrorPassword:           "黑月编译失败",
	EclErrorEC:                 "源码密码不正确",
	EclErrorELib:               "缺乏易模块",
	EclErrorStartTimeout:       "缺少支持库",
	EclErrorCompileTimeout:     "启动易语言超时",
	EclErrorNotSupportEPkg:     "不支持易包编译",
}

func EclErrorTips(code int) string {
	return eclErrorTips[code]
}

type EclCmd struct {
	exec *Exec
}

func NewEclCmd(path string, args ...string) *EclCmd {
	return &EclCmd{
		exec: NewExec(path, args...),
	}
}

var eclMatchError = regexp.MustCompile("\\(错误:(-\\d+)\\)(.+)")

func (c *EclCmd) OnLog(onLog ReportFunc) {
	c.exec.OnLog(func(s string) {
		if eclMatchError.MatchString(s) {
			sm := eclMatchError.FindStringSubmatch(s)
			c.exec.onError(sm[2])
			code, _ := strconv.Atoi(sm[1])
			c.exec.onExit(code)
		} else {
			onLog(s)
		}
	})
}

func (c *EclCmd) OnError(onError ReportFunc) {
	c.exec.OnError(onError)
}

func (c *EclCmd) OnOver(onOver func()) {
	c.exec.OnOver(onOver)
}

func (c *EclCmd) Exec() {
	c.exec.Exec()
}

func (c *EclCmd) Wait() {
	c.exec.Wait()
}
