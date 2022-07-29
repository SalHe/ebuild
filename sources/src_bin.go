package sources

type ESourceType int32
type EBuildTargetType int32

const (
	ESourceSrc  ESourceType = 1
	ESourceECom ESourceType = 3

	EBuildTargetWindowsForm    EBuildTargetType = 0
	EBuildTargetWindowsConsole EBuildTargetType = 1
	EBuildTargetWindowsDll     EBuildTargetType = 2
	EBuildTargetWindowsECom    EBuildTargetType = 1000
	EBuildTargetLinuxConsole   EBuildTargetType = 10000
	EBuildTargetLinuxECom      EBuildTargetType = 11000
)

func (b *EBuildTargetType) IsWindowsExecutable() bool {
	return *b == EBuildTargetWindowsForm || *b == EBuildTargetWindowsConsole
}

func (b *EBuildTargetType) Ext() string {
	switch *b {
	case EBuildTargetWindowsForm:
	case EBuildTargetWindowsConsole:
		return ".exe"
	case EBuildTargetWindowsDll:
		return ".dll"
	case EBuildTargetWindowsECom:
	case EBuildTargetLinuxECom:
		return ".ec"
	case EBuildTargetLinuxConsole:
		return ""
	default:
		return ""
	}

	return ""
}
