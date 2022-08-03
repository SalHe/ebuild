# 基本配置

## 工程描述信息

```yaml
project:
  name: ebuild-example                          # 工程名
  version: "1.0"                                # 工程版本
  description: 这是由ebuild创建的示例工程。       # 工程的描述信息    
  author: SalHe                                 # 工程作者
  repository: https://github.com/SalHe/ebuild   # 工程的源码仓库地址
  homepage: https://github.com/SalHe            # 工程主页
```

您可以在`ebuild.yaml`中看到上述的内容，在`project`之下有若干用于描述工程的信息，他们对工程没有任何影响，仅仅是用于描述工程，方便了解工程的信息。


## 管理源码

在`ebuild`中，并不是所有源码都会参与`e2txt`、`txt2e`、编译等的过程，只有那些按照约定包含(includes)进来但又没有被排除掉的文件(excludes)的文件才能参与这些过程。

### 被管理的源文件

```yaml
project:
  # ......
includes:
  - '**/*.e'
```

在`includes`节点中，您可以配置多种文件名匹配模式来选择你需要被纳入控制的源码。在上述配置中，要求工程管理所有目录(`**/`)下的易语言源文件(`*.e`)。

### 排除的源文件

有时候，您希望在工程中排除一些源文件，使他们不纳入`ebuild`的控制——既不参与`e2txt`也不参与编译。那么您可以配置`excludes`。

```yaml
project:
  # ......
excludes:
  - '**/*.recover.e'
  - '**/*.ecode/*.e'
  - '**/*.代码/*.e'
```

上述代码中，排除了三种模式的文件，比如`XXX.recover.e`被排除掉了。

`excludes`的优先级是高于`includes`的，即：纵使`includes`中包含了`XXX.recover.e`，他也不会被纳入`ebuild`的控制。

### 仅在构建中排除源文件

```yaml
project:
  # ......
exclude-builds:
  - './scripts/**/*.e' # 脚本文件不纳入'ebuild build'命令中进行自动构建
```

与`excludes`不同，有些源文件您希望参与`e2txt`，但不希望他作为编译目标——作为脚本的易语言源文件便是一个例子，上述配置使得`scripts`目录下的所有易语言源文件`*.e`都不参与编译，但是他们仍然参与`e2txt`。

`exclude-builds`的优先级高于`includes`，低于`excludes`，即：倘若`excludes`排除了指定源文件，即便该配置中未排除那个文件，该文件最终仍然被排除（既不参与`e2txt`也不参与编译）。