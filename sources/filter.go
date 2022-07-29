package sources

func FilterESrcs(esrcs []*Source, filter func(src *Source) bool) (srcs []*Source) {
	checked := make(map[*Source]bool)

	for _, esrc := range esrcs {
		if !checked[esrc] && filter(esrc) {
			srcs = append(srcs, esrc)
			checked[esrc] = true
		}
	}
	return
}

func FilterTargets(targets []string) func(src *Source) bool {
	return func(src *Source) bool {
		// TODO 尝试优化
		for _, target := range targets {
			if src.Match(target) {
				return true
			}
		}
		return false
	}
}

func FilterRmNoBuild() func(src *Source) bool {
	return func(src *Source) bool {
		return !src.NoBuild()
	}
}
