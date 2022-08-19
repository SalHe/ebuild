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
          { text: '什么是 ebuild？', link: '/README' },
          { text: '安装', link: '/installation' },
          { text: '第一个工程', link: '/first-project' },
          { text: '鸣谢', link: '/thanks' },
        ]
      },
      {
        text: '工程配置',
        items: [
          { text: '基本配置', link: '/project/basic' },
          { text: '工程脚本 - 重复工作', link: '/project/run' },
          { text: 'e2txt配置', link: '/project/e2txt' },
          { text: '构建配置', link: '/project/build' },
          { text: '环境变量', link: '/project/environ' },
          { text: '案例', link: '/project/examples' },
        ]
      },
      {
        text: '命令行帮助',
        items: [
          { text: '概览', link: '/cli/' },
        ]
      }
    ],
    editLink: {
      pattern: 'https://github.com/SalHe/ebuild/edit/master/docs/:path',
      text: '在 GitHub 上编辑'
    },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/SalHe/ebuild' },
    ],
    algolia: {
      appId: 'I0YZBJE735',
      apiKey: 'be48f498e8c14176450fe574c8347f12',
      indexName: 'ebuild'
    },
  },
  markdown: {
    config: (md) => {
      md.use(miCheckbox, { readonly: true })
    }
  }
})
