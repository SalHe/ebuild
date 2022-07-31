package hooks

type EBuildPeriod string

const (
	PeriodPreBuild  EBuildPeriod = "pre-build"
	PeriodPostBuild EBuildPeriod = "post-build"
)
