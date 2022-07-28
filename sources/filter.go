package sources

func FilterESrcs(esrcs []*Source, targets []string) (srcs []*Source) {
	// TODO 尝试优化
	checked := make(map[*Source]bool)
	for _, target := range targets {
		for _, esrc := range esrcs {
			if !checked[esrc] && esrc.Match(target) {
				srcs = append(srcs, esrc)
				checked[esrc] = true
			}
		}
	}
	return
}
