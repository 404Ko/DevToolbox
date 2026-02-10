using DevToolbox.Shared.Models;

namespace DevToolbox.Shared.Constants;

/// <summary>
/// Tool categories configuration (AI Accept)
/// </summary>
public static class ToolCategoriesConfig
{
    public static List<ToolCategory> GetCategories() => new()
    {
        new ToolCategory
        {
            Key = "encoding",
            Name = "编码/解码",
            Icon = "📦",
            IsExpanded = true,
            Tools = new List<ToolItem>
            {
                new("Base64 编码/解码", "/encoding/base64"),
                new("Base64 转文件", "/encoding/base64-file"),  // AI Accept [Added]
                new("URL 编码/解码", "/encoding/url"),
                //new("HTML 实体编码", "/encoding/html"),
                //new("Unicode 编码", "/encoding/unicode"),
                //new("Hex 编码", "/encoding/hex"),
            }
        },
        new ToolCategory
        {
            Key = "crypto",
            Name = "加密/解密",
            Icon = "🔐",
            IsExpanded = false,
            Tools = new List<ToolItem>
            {
                new("Hash 计算", "/crypto/hash"),
                new("AES 加密/解密", "/crypto/aes"),
                new("JWT 解析", "/crypto/jwt"),
            }
        },
        new ToolCategory
        {
            Key = "formatter",
            Name = "格式化",
            Icon = "📝",
            IsExpanded = false,
            Tools = new List<ToolItem>
            {
                new("JSON 格式化", "/formatter/json"),
                new("XML 格式化", "/formatter/xml"),
                new("去除HTML标签", "/formatter/strip-html"),
            }
        },
        new ToolCategory
        {
            Key = "time",
            Name = "时间工具",
            Icon = "⏰",
            IsExpanded = false,
            Tools = new List<ToolItem>
            {
                new("时间戳转换", "/time/timestamp"),
                new("日期计算器", "/time/calculator"),
            }
        },
        new ToolCategory
        {
            Key = "generator",
            Name = "生成器",
            Icon = "🎲",
            IsExpanded = false,
            Tools = new List<ToolItem>
            {
                new("UUID/GUID 生成", "/generator/uuid"),
                new("随机字符串", "/generator/random-string"),
                new("密码生成器", "/generator/password"),
            }
        },
        new ToolCategory
        {
            Key = "other",
            Name = "其他工具",
            Icon = "🔧",
            IsExpanded = false,
            Tools = new List<ToolItem>
            {
                new("正则测试", "/other/regex"),
                new("进制转换", "/other/radix"),
                new("颜色转换", "/other/color"),
                new("字符统计", "/other/char-count"),
            }
        }
    };
}
