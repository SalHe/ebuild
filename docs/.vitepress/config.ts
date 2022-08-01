import { defineConfig } from "vitepress";
import miCheckbox from "markdown-it-task-checkbox";
import { version } from '../../package.json'

export default defineConfig({
  lang: 'zh-CN',
  base: '/ebuild/',
  title: 'EBuild',
  description: 'EBuild 是一个专注于易语言的构建工具。',
  lastUpdated: true,
  themeConfig: {
    outlineTitle: '目录',
    footer: {
      message: '本开源软件受 MIT 协议保护',
      copyright: 'Copyright (c) 2022 SalHe Li'
    },

    nav: [
      { text: '使用向导', link: '/README' },
      { text: '命令行帮助', link: '/cli/' },
      {
        text: '友情链接',
        items: [
          { text: "SalHe's Home", link: "https://salhe.github.io" },
          { text: "SalHe's Blog", link: "https://salhe.github.io/blog" }
        ]
      }
    ],
    sidebar: [
      {
        text: '介绍',
        items: [
          { text: '什么是 ebuild？', link: '/README' }
        ]
      },
      {
        text: '命令行帮助',
        items: cliItems(
          { subCommand: 'init', title: '初始化工程' },
          { subCommand: 'info', title: '查看工程' },
          { subCommand: 'e2txt', title: '易转文本' },
          { subCommand: 'txt2e', title: '文本转易' },
          { subCommand: 'build', title: '构建工程' },
          { subCommand: 'clean', title: '清理工程' },
          { subCommand: 'run', title: '运行脚本' },
          { subCommand: 'toolchain', title: '检查工具链' },
        )
      }
    ],
    editLink: {
      pattern: 'https://github.com/SalHe/ebuild/edit/master/docs/:path',
      text: '在 GitHub 上编辑'
    },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/SalHe/ebuild' },
    ],
  },
  markdown: {
    config: (md) => {
      md.use(miCheckbox, { readonly: true })
    }
  }
})

interface SubCommand {
  subCommand: string,
  title?: string
}

function cliItems(...subCommands: SubCommand[]) {
  return subCommands.map(x => ({
    text: x.title ? `${x.title} - ${x.subCommand}` : x.subCommand,
    link: `/cli/#ebuild-${x.subCommand}`
  }))
}