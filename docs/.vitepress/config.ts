import { defineConfig } from "vitepress";
import miCheckbox from "markdown-it-task-checkbox";

export default defineConfig({
  base: '/ebuild/',
  title: 'ebuild',
  description: 'ebuild 是一个专注于易语言的构建工具。',
  markdown: {
    config: (md) => {
      md.use(miCheckbox, { readonly: true })
    }
  }
})