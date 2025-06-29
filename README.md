# HTML 转换器 (Html-Converter)

一个能将 HTML 文件快速转换为**符合 VCB-Studio 发布规范**的 Markdown 或 BBCode 的跨平台小工具。

转换结果可自动复制到剪贴板，方便直接粘贴。

---

## 如何使用

1.  **下载** -> 前往 [**Releases**](https://github.com/demo0102/Html-Converter/releases) 页面下载最新程序。

2.  **转换文件** -\> 您可以通过以下任意一种方式来指定需要转换的 HTML 文件：

    #### 方法一：交互式拖拽 (推荐)

    1.  直接运行程序 (双击 `Html-Converter.exe` 或在终端中执行 `./Html-Converter`)。
    2.  将您的 `.html` 文件从文件管理器中，直接拖拽到**终端窗口**里。

    #### 方法二：快捷拖拽 (仅限 Windows)

    在 Windows 上，您可以直接将 `.html` 文件拖拽到 `Html-Converter.exe` 的**程序图标**上，即可一步完成启动和文件路径的指定。

    #### 方法三：命令行

    1.  打开终端 (CMD, PowerShell, Terminal 等)。
    2.  使用格式 `程序名 "文件路径"` 来运行。
          * **Windows 示例:**
            ```shell
            .\Html-Converter.exe "C:\Users\Downloads\MyFile.html"
            ```
          * **macOS / Linux 示例:**
            ```shell
            ./Html-Converter "/Users/yourname/Downloads/MyFile.html"
            ```
    -----

    在程序成功获取文件路径后，只需根据提示选择 `1` (Markdown) 或 `2` (BBCode) 模式，即可完成转换。

3.  **粘贴** -> 到论坛编辑器中直接粘贴即可。

---

## 关于源码

如需自行编译，克隆仓库后使用 Visual Studio 打开 `.sln` 文件即可。