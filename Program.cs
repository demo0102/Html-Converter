using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

class Program
{
    enum ConversionMode
    {
        Markdown,
        BBCode
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // 获取输入路径
        string filePath = GetFilePath(args);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("文件不存在，请检查路径是否正确");
            return;
        }

        // 验证文件后缀
        if (!IsValidHtmlFile(filePath))
        {
            Console.WriteLine("错误：只支持HTML文件（.html 或 .htm 后缀）");
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
            return;
        }

        // 选择转换模式
        ConversionMode mode = GetConversionMode();

        try
        {
            // 读取文件
            string html = File.ReadAllText(filePath, Encoding.UTF8);

            // 转换处理
            string result;
            string extension;

            if (mode == ConversionMode.Markdown)
            {
                result = ConvertHtmlToMarkdown(html);
                extension = ".md";
            }
            else
            {
                result = ConvertHtmlToBBCode(html);
                extension = ".bbcode";
            }

            // 生成输出路径
            string outputPath = Path.Combine(
                Path.GetDirectoryName(filePath) ?? "",
                $"{Path.GetFileNameWithoutExtension(filePath)}{extension}"
            );

            // 保存文件
            File.WriteAllText(outputPath, result, Encoding.UTF8);
            Console.WriteLine($"转换完成！输出文件：{outputPath}");

            // 询问是否复制到剪贴板
            if (AskToCopyToClipboard())
            {
                if (CopyToClipboard(result))
                {
                    Console.WriteLine("✓ 内容已复制到剪贴板");
                }
                else
                {
                    Console.WriteLine("✗ 复制到剪贴板失败");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"处理出错：{ex.Message}");
        }

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    static string GetFilePath(string[] args)
    {
        if (args.Length > 0)
        {
            return args[0].Trim('"'); // 处理带空格的路径
        }

        Console.Write("请拖入HTML文件或输入路径：");
        return Console.ReadLine()?.Trim('"') ?? "";
    }

    static bool IsValidHtmlFile(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        return extension == ".html" || extension == ".htm";
    }

    static ConversionMode GetConversionMode()
    {
        Console.WriteLine("请选择转换模式：");
        Console.WriteLine("1. Markdown (默认)");
        Console.WriteLine("2. BBCode");
        Console.Write("请输入选择 (直接回车默认选择Markdown)：");

        string input = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrEmpty(input) || input == "1")
        {
            Console.WriteLine("选择了 Markdown 模式");
            return ConversionMode.Markdown;
        }
        else if (input == "2")
        {
            Console.WriteLine("选择了 BBCode 模式");
            return ConversionMode.BBCode;
        }
        else
        {
            Console.WriteLine("无效输入，默认选择 Markdown 模式");
            return ConversionMode.Markdown;
        }
    }

    static bool AskToCopyToClipboard()
    {
        Console.Write("是否复制到剪贴板？(Y/n，默认为是)：");
        string input = Console.ReadLine()?.Trim().ToLower() ?? "";
        return string.IsNullOrEmpty(input) || input == "y" || input == "yes";
    }

    static bool CopyToClipboard(string text)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CopyToClipboardWindows(text);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return CopyToClipboardMacOS(text);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return CopyToClipboardLinux(text);
            }
            else
            {
                Console.WriteLine("不支持的操作系统");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"复制失败：{ex.Message}");
            return false;
        }
    }

    static bool CopyToClipboardWindows(string text)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "clip",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process?.StandardInput != null)
                {
                    using (var writer = process.StandardInput)
                    {
                        writer.Write(text);
                    }
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    static bool CopyToClipboardMacOS(string text)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "pbcopy",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process?.StandardInput != null)
                {
                    using (var writer = process.StandardInput)
                    {
                        writer.Write(text);
                    }
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    static bool CopyToClipboardLinux(string text)
    {
        try
        {
            // 尝试 xclip
            var startInfo = new ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process?.StandardInput != null)
                {
                    using (var writer = process.StandardInput)
                    {
                        writer.Write(text);
                    }
                    process.WaitForExit();
                    if (process.ExitCode == 0) return true;
                }
            }
        }
        catch { }

        try
        {
            // 如果 xclip 失败，尝试 xsel
            var startInfo = new ProcessStartInfo
            {
                FileName = "xsel",
                Arguments = "--clipboard --input",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process?.StandardInput != null)
                {
                    using (var writer = process.StandardInput)
                    {
                        writer.Write(text);
                    }
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    static string ConvertHtmlToMarkdown(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        string markdown = ProcessNodeForMarkdown(doc.DocumentNode);

        // 处理行首空格
        string[] lines = markdown.Split(new[] { '\n' }, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimStart();
        }
        return FormatMarkdownSpacing(string.Join("\n", lines));
    }

    static string ProcessNodeForMarkdown(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Document)
        {
            StringBuilder sbDoc = new StringBuilder();
            foreach (var child in node.ChildNodes)
            {
                sbDoc.Append(ProcessNodeForMarkdown(child));
            }
            return sbDoc.ToString();
        }
        if (node.NodeType == HtmlNodeType.Text)
        {
            string text = node.InnerText;
            // 合并连续空白但保留换行
            return Regex.Replace(text, @"[ \t]+", " ").Replace("\n", " ");
        }
        else if (node.NodeType == HtmlNodeType.Element)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var child in node.ChildNodes)
            {
                sb.Append(ProcessNodeForMarkdown(child));
            }
            string innerContent = sb.ToString();

            switch (node.Name.ToLower())
            {
                case "img":
                    string alt = node.GetAttributeValue("alt", "");
                    string src = node.GetAttributeValue("src", "");
                    return $"![{alt}]({src})";
                case "a":
                    string href = node.GetAttributeValue("href", "");
                    if (href.StartsWith("http"))
                    {
                        return $"[{innerContent}]({href})";
                    }
                    else
                    {
                        return $"[![]({innerContent})]({href})";
                    }
                case "br":
                    return "  \n";
                case "strong":
                case "b":
                    return $"**{innerContent}**";
                case "em":
                case "i":
                    return $"*{innerContent}*";
                case "p":
                    string content = innerContent.Trim();
                    if (string.IsNullOrEmpty(content))
                        return "\n";
                    return content + "  \n";
                case "hr":
                    return "\n* * *\n";
                case "h1":
                    return $"# {innerContent}\n";
                case "h2":
                    return $"## {innerContent}\n";
                case "h3":
                    return $"### {innerContent}\n";
                case "h4":
                    return $"#### {innerContent}\n";
                case "h5":
                    return $"##### {innerContent}\n";
                case "h6":
                    return $"###### {innerContent}\n";
                default:
                    return innerContent;
            }
        }
        return "";
    }

    static string FormatMarkdownSpacing(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return markdown;

        // 按换行符分割为行数组
        string[] lines = markdown.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        // 去除开头空行
        int startIndex = 0;
        while (startIndex < lines.Length && string.IsNullOrWhiteSpace(lines[startIndex]))
        {
            startIndex++;
        }

        if (startIndex >= lines.Length) return string.Empty;

        // 去除结尾空行
        int endIndex = lines.Length - 1;
        while (endIndex >= 0 && string.IsNullOrWhiteSpace(lines[endIndex]))
        {
            endIndex--;
        }

        // 创建格式化后的行列表
        var formattedLines = new List<string>();
        for (int i = startIndex; i <= endIndex; i++)
        {
            string line = lines[i];

            // 处理分割线前的空行
            if (line.Trim() == "* * *")
            {
                // 确保分割线前有两行空行
                if (formattedLines.Count > 0 && !string.IsNullOrWhiteSpace(formattedLines[formattedLines.Count - 1]))
                {
                    formattedLines.Add("");
                    formattedLines.Add("");
                }
                else if (formattedLines.Count > 0 && string.IsNullOrWhiteSpace(formattedLines[formattedLines.Count - 1]))
                {
                    formattedLines.Add("");
                }
                formattedLines.Add(line);

                // 在分割线后添加空行
                if (i < endIndex)
                {
                    formattedLines.Add("");
                }
            }
            // 处理包含下划线的Source/Encode行
            else if (line.Contains("_") && line.Contains("Source") && line.Contains("Encode"))
            {
                formattedLines.Add(line);
                // 在这行后面添加空行
                if (i < endIndex)
                {
                    formattedLines.Add("");
                }
            }
            else
            {
                formattedLines.Add(line);
            }
        }

        return string.Join("\n", formattedLines);
    }

    static string ConvertHtmlToBBCode(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        string bbcode = ProcessNodeForBBCode(doc.DocumentNode);

        // 新增行首空格处理
        string[] lines = bbcode.Split(new[] { '\n' }, StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimStart();
        }
        return FormatBBCodeSpacing(string.Join("\n", lines));
    }

    static string ProcessNodeForBBCode(HtmlNode node)
    {
        if (node.NodeType == HtmlNodeType.Document)
        {
            StringBuilder sbDoc = new StringBuilder();
            foreach (var child in node.ChildNodes)
            {
                sbDoc.Append(ProcessNodeForBBCode(child));
            }
            return sbDoc.ToString();
        }
        if (node.NodeType == HtmlNodeType.Text)
        {
            string text = node.InnerText;
            // 合并连续空白但保留换行
            return Regex.Replace(text, @"[ \t]+", " ").Replace("\n", " ");
        }
        else if (node.NodeType == HtmlNodeType.Element)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var child in node.ChildNodes)
            {
                sb.Append(ProcessNodeForBBCode(child));
            }
            string innerContent = sb.ToString();
            switch (node.Name.ToLower())
            {
                case "img":
                    return $"[img]{node.GetAttributeValue("src", "")}[/img]";
                case "a":
                    return $"[url={node.GetAttributeValue("href", "")}]{innerContent}[/url]";
                case "br":
                    return "\n";
                case "strong":
                    return $"[b]{innerContent}[/b]";
                case "p":
                    return "\n" + innerContent.Trim() + "\n";
                case "hr":
                    return "";
                default:
                    return innerContent;
            }
        }
        return "";
    }

    static string FormatBBCodeSpacing(string bbcode)
    {
        if (string.IsNullOrWhiteSpace(bbcode)) return bbcode;

        // 按换行符分割为行数组（兼容不同系统换行符）
        string[] lines = bbcode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        // 去除开头空行
        int startIndex = 0;
        while (startIndex < lines.Length && string.IsNullOrWhiteSpace(lines[startIndex]))
        {
            startIndex++;
        }

        // 处理全空行的情况
        if (startIndex >= lines.Length) return string.Empty;

        // 去除结尾空行并添加单个空行
        int endIndex = lines.Length - 1;
        while (endIndex >= 0 && string.IsNullOrWhiteSpace(lines[endIndex]))
        {
            endIndex--;
        }

        // 创建新数组（保留中间空行）
        var formattedLines = new List<string>();
        for (int i = startIndex; i <= endIndex; i++)
        {
            formattedLines.Add(lines[i]);
        }

        // 添加结尾空行
        formattedLines.Add("");

        // 用系统换行符合并
        return string.Join(Environment.NewLine, formattedLines);
    }
}