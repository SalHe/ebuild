# 构建(编译)

您可以全局性的指定编译源文件时采用的编译方式，如：独立编译、静态编译等。也可以单独为目标(`target`)指定构建配置。

对于未指定构建配置的目标将采用全局的构建配置。


## 全局配置

```yaml
project:
  # ......
build:
  compiler: 独立编译
```

在上述配置中，指定了全局默认的构建配置采用的编译方式为“独立编译”，您可以指定其他的[编译方式](#编译方式)。

## 编译方式

支持的编译方式如下：

- 黑月编译
- 黑月汇编
- 黑月C++
- 黑月MFC
- 静态编译
- 独立编译

::: info
编译方式定义于: https://github.com/SalHe/ebuild/blob/master/config/source.go
:::

## 特定目标配置

您可以在`targets`下配置若干构建目标，对目标的输出文件名、编译方式等做出说明。

配置格式如下：

```yaml
projects:
  # ......
targets:
  - name: <目标的名称>
    description: <目标的描述>
    source: <目标的源文件路径>
    build:
      compiler: <编译方式>
    output: <输出文件名> # 当使用相对路径时，将相对于构建输出目录
    package: false # 是否为易包
    hooks: # 构建生命周期相关脚本
      pre-build: <编译该目标前执行的脚本>
      post-build: <编译该目标前之后的脚本>
  - name: ...
    ...

```

### 构建生命周期相关脚本

`ebuild`将对目标的构建拆分成了若干生命周期，您可以在生命周期中指定执行一些代码。比如编译完成后，您可以使用脚本将输出文件复制到某处，或者对文件创建副本等。

`ebuild`在执行这些脚本时也会传入一些额外的环境变量，用于在脚本中获得当前构建目标的一些信息。

目前的生命周期主要有：

- pre-build: 在编译目标之前执行
- post-build: 在编译目标之后执行