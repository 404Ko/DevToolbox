# DevToolbox - 开发者在线工具箱

> 基于 Blazor WebAssembly 构建的纯前端开发者工具箱，无需后端服务，所有计算在浏览器端完成。

## 技术栈

| 项目 | 技术 |
|------|------|
| 框架 | Blazor WebAssembly (.NET 8.0) |
| UI | Bootstrap 5 + 自定义 CSS |
| 架构 | 组件化 + 服务层 DI |
| 部署 | 纯静态文件，支持 GitHub Pages / Nginx / CDN |

## 功能一览

### 📦 编码/解码

| 工具 | 路由 | 说明 |
|------|------|------|
| Base64 编码/解码 | `/encoding/base64` | UTF-8 文本与 Base64 互转 |
| Base64 转文件 | `/encoding/base64-file` | 文件与 Base64 互转：上传文件生成 Base64，或粘贴 Base64 还原为文件下载；支持 16+ 格式 MIME 自动检测、Data URI、图片预览、后缀手动修改 |
| URL 编码/解码 | `/encoding/url` | URL 特殊字符编码/解码 |

### 🔐 加密/解密

| 工具 | 路由 | 说明 |
|------|------|------|
| Hash 计算 | `/crypto/hash` | MD5 / SHA1 / SHA256 / SHA512 |
| AES 加密/解密 | `/crypto/aes` | 支持 128/192/256 位密钥，CBC/ECB 模式 |
| JWT 解析 | `/crypto/jwt` | Header/Payload 解析，过期检测，Claims 表格展示 |

### 📝 格式化

| 工具 | 路由 | 说明 |
|------|------|------|
| JSON 格式化 | `/formatter/json` | 格式化美化 / 压缩 / 复制；支持单引号 JSON 自动转换；内置 JSON → C# 实体映射验证（逐字段类型匹配、模糊建议） |
| XML 格式化 | `/formatter/xml` | XML 格式化 / 压缩；内置 XML → C# 实体映射验证 |
| 去除 HTML 标签 | `/formatter/strip-html` | 纯文本提取、保留换行模式、标签统计分析 |

### ⏰ 时间工具

| 工具 | 路由 | 说明 |
|------|------|------|
| 时间戳转换 | `/time/timestamp` | Unix 时间戳与日期互转，秒/毫秒切换，当前时间实时刷新（1s） |
| 日期计算器 | `/time/calculator` | 两日期差值计算（天/时/分/秒）；日期加减运算（±年/月/日/时/分/秒） |

### 🎲 生成器

| 工具 | 路由 | 说明 |
|------|------|------|
| UUID/GUID 生成 | `/generator/uuid` | 多格式 GUID 批量生成 |
| 随机字符串 | `/generator/random-string` | 可配置字符集（大小写/数字/特殊字符/自定义）、长度 1-1024、分隔符、字符池预览 |
| 密码生成器 | `/generator/password` | 可配置字符类型、排除易混淆字符、快速预设（8/16/24/32位）、批量生成、基于熵的强度评估、独立密码强度检测器 |

### 🔧 其他工具

| 工具 | 路由 | 说明 |
|------|------|------|
| 正则测试 | `/other/regex` | 支持 i/m/s/g 标志位、分组捕获展示、替换预览、5s 超时保护、示例加载 |
| 进制转换 | `/other/radix` | 二/八/十/十六进制互转、前缀自动识别（0x/0b/0o）、4位二进制分组、常用值参考表 |
| 颜色转换 | `/other/color` | HEX / RGB / HSL 三种输入模式、实时颜色预览、12 款预设色板 |
| 字符统计 | `/other/char-count` | 6 维统计（总字符/无空格/中文/字母/数字/行数）、字符分类表、Top 30 频率柱状图 |

## 项目结构

```
src/
├── DevToolbox.Web/                 # Blazor WebAssembly 主项目
│   ├── Pages/                      # 工具页面组件
│   │   ├── Encoding/               #   编码/解码工具
│   │   │   ├── Base64Tool.razor
│   │   │   ├── Base64FileTool.razor
│   │   │   └── UrlTool.razor
│   │   ├── Crypto/                 #   加密/解密工具
│   │   │   ├── HashTool.razor
│   │   │   ├── AesTool.razor
│   │   │   └── JwtTool.razor
│   │   ├── Formatter/              #   格式化工具
│   │   │   ├── JsonTool.razor
│   │   │   ├── XmlTool.razor
│   │   │   └── StripHtmlTool.razor
│   │   ├── Time/                   #   时间工具
│   │   │   ├── TimestampTool.razor
│   │   │   └── DateCalculatorTool.razor
│   │   ├── Generator/              #   生成器工具
│   │   │   ├── UuidTool.razor
│   │   │   ├── RandomStringTool.razor
│   │   │   └── PasswordTool.razor
│   │   └── Other/                  #   其他工具
│   │       ├── RegexTool.razor
│   │       ├── RadixTool.razor
│   │       ├── ColorTool.razor
│   │       └── CharCountTool.razor
│   ├── Services/                   # 业务逻辑服务层
│   │   ├── EncodingService.cs      #   Base64/URL/HTML/Unicode/Hex 编解码 + 文件 Base64 + MIME 检测
│   │   ├── CryptoService.cs        #   Hash + AES 加解密
│   │   ├── JwtService.cs           #   JWT Token 解析
│   │   ├── FormatterService.cs     #   JSON/XML 格式化/压缩 + 实体映射验证
│   │   ├── TimeService.cs          #   时间戳转换 + 日期计算
│   │   ├── GeneratorService.cs     #   UUID/随机字符串/密码生成 + 强度评估
│   │   ├── OtherToolService.cs     #   正则测试/进制转换/颜色转换/字符统计
│   │   └── ManagedMd5.cs           #   纯托管 MD5 (WASM 兼容)
│   ├── Layout/                     # 布局组件
│   └── wwwroot/                    # 静态资源
│
└── DevToolbox.Shared/              # 共享类库
    ├── Models/                     #   数据模型
    └── Constants/                  #   工具分类配置
```

## 服务层设计

所有业务逻辑通过**接口 + 实现**的方式组织，通过 DI 注入到页面组件：

| 服务接口 | 职责 | 主要方法 |
|----------|------|----------|
| `IEncodingService` | 编码/解码 | Base64/URL/HTML/Unicode/Hex 编解码、文件 Base64 互转、MIME 魔数检测、文件扩展名映射 |
| `ICryptoService` | 加密/哈希 | MD5/SHA 哈希、AES 加解密、随机密钥生成 |
| `IJwtService` | JWT 解析 | Token 解析、Claims 提取、过期检测 |
| `IFormatterService` | 格式化/验证 | JSON 格式化/压缩（支持单引号）、XML 格式化/压缩、JSON/XML → C# 实体映射验证 |
| `ITimeService` | 时间工具 | 时间戳 ↔ 日期互转、日期差值计算、日期加减运算 |
| `IGeneratorService` | 内容生成 | UUID 生成、随机字符串（自定义字符集）、安全密码（可配置规则）、密码强度评估 |
| `IOtherToolService` | 综合工具 | 正则测试（含替换/分组）、进制转换、RGB/HSL/HEX 颜色转换、字符频率统计 |

## 快速开始

### 环境要求

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 运行

```bash
cd src/DevToolbox.Web
dotnet run
```

浏览器访问 `https://localhost:5001`

### 发布

```bash
dotnet publish src/DevToolbox.Web -c Release -o publish
```

发布产物在 `publish/wwwroot/` 目录下，全部为静态文件，可直接部署到任意 Web 服务器。

## 特殊说明

### WASM 兼容性

- **MD5**：浏览器 SubtleCrypto 不支持 MD5，项目使用纯 C# 实现的 `ManagedMd5`（RFC 1321）
- **SHA1/SHA256/SHA512**：通过 `Create().ComputeHash()` 实例方法调用，兼容 WASM 运行时
- **AES**：`Aes.Create()` 在部分浏览器可能不支持，页面已做异常处理

### 安全特性

- 所有数据处理在浏览器端完成，不发送任何网络请求
- JWT 解析仅解码显示，不进行签名验证（无需密钥）
- AES 密钥和密码生成使用 `RandomNumberGenerator`，密码学安全

### JSON 容错

- 支持尾部逗号（trailing commas）
- 支持 JSON 注释（`//` 和 `/* */`）
- 支持单引号 JSON 自动转换为双引号（兼容 Python dict / JS 对象格式）

### MIME 文件类型检测

Base64 转文件工具通过魔数（magic bytes）自动识别以下格式：

| 类别 | 支持格式 |
|------|----------|
| 图片 | PNG, JPEG, GIF, WebP, BMP, SVG, ICO |
| 文档 | PDF, JSON, XML, HTML |
| 压缩 | ZIP, GZIP |
| 音视频 | MP3, WAV, OGG, MP4 |
| 文本 | 自动识别纯文本（启发式检测：<5% 非打印字符即判定为文本） |
