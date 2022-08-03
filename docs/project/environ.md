# 环境变量

在[执行脚本](./run.md#工程相关脚本)、[执行易语言源文件](./run.md#使用易语言程序作为脚本)以及[执行构建目标生命周期脚本](./build.md#构建生命周期相关脚本)的时候，`ebuild`会设置一些环境变量用于辅助脚本的编写，以下对所设置的环境变量做出说明。

| 环境变量名                | 说明                   | 执行脚本/易语言源文件 | 构建生命周期 |
| ------------------------- | ---------------------- | --------------------- | ------------ |
| EBUILD_EXECUTABLE_PATH    | `ebuild`可执行文件路径 | ✔                     | ✔            |
| ELANG_DIR                 | `易语言`安装目录       | ✔                     | ✔            |
| ECL_DIR                   | `ecl`安装目录          | ✔                     | ✔            |
| E2TXT_DIR                 | `e2txt`安装目录        | ✔                     | ✔            |
| EBUILD_PROJECT_ROOT_DIR   | 工程根目录             | ✔                     | ✔            |
| EBUILD_PROJECT_OUTPUT_DIR | 构建输出目录           | ✔                     | ✔            |
| EBUILD_PERIOD             | 构建生命周期           | ❌                     | ✔            |
| EBUILD_SOURCE_FILE        | 被构建的源文件         | ❌                     | ✔            |
| EBUILD_TARGET_FILE        | 构建目标输出路径       | ❌                     | ✔            |
| EBUILD_TARGET_TYPE        | 构建目标类型           | ❌                     | ✔            |