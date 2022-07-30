package hooks

type EBuildPeriod string

const (
	PeriodPreBuild  EBuildPeriod = "PreBuild"
	PeriodPostBuild EBuildPeriod = "PostBuild"
)
