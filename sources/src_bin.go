package sources

type eSourceType int32
type eBuildTargetType int32

const (
	ESourceSrc  eSourceType = 1
	ESourceECom eSourceType = 3

	EBuildTargetWindowsForm    eBuildTargetType = 0
	EBuildTargetWindowsConsole eBuildTargetType = 1
	EBuildTargetWindowsDll     eBuildTargetType = 2
	EBuildTargetWindowsECom    eBuildTargetType = 1000
	EBuildTargetLinuxConsole   eBuildTargetType = 10000
	EBuildTargetLinuxECom      eBuildTargetType = 11000
)

func (b *eBuildTargetType) Ext() string {
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
