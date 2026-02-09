# 🔧 DevToolbox - 开发者在线工具箱

> 基于 Blazor WebAssembly 构建的纯前端开发者工具箱，无需后端服务，所有计算在浏览器端完成。

## 技术栈

| 项目 | 技术 |
|------|------|
| 框架 | Blazor WebAssembly (.NET 8.0) |
| UI | Bootstrap 5 + 自定义 CSS |
| 架构 | 组件化 + 服务层 DI |
| 部署 | 纯静态文件，支持 GitHub Pages / Nginx / CDN |

## 功能一览

### ✅ 已实现

| 分类 | 工具 | 路由 | 说明 |
|------|------|------|------|
| 📦 编码/解码 | Base64 编码/解码 | `/encoding/base64` | UTF-8 文本与 Base64 互转 |
| | URL 编码/解码 | `/encoding/url` | URL 特殊字符编码/解码 |
| 🔐 加密/解密 | Hash 计算 | `/crypto/hash` | MD5 / SHA1 / SHA256 / SHA512 |
| | AES 加密/解密 | `/crypto/aes` | 支持 128/192/256 位密钥，CBC/ECB 模式 |
| | JWT 解析 | `/crypto/jwt` | Header/Payload 解析，过期检测，Claims 展示 |
| 📝 格式化 | JSON 格式化 | `/formatter/json` | 格式化美化 / 压缩 / 复制 |
| ⏰ 时间工具 | 时间戳转换 | `/time/timestamp` | Unix 时间戳与日期互转 |
| 🎲 生成器 | UUID/GUID 生成 | `/generator/uuid` | 多格式 GUID 批量生成 |

### 🚧 规划中

| 分类 | 工具 | 路由 |
|------|------|------|
| 📝 格式化 | XML 格式化 | `/formatter/xml` |
| | 去除 HTML 标签 | `/formatter/strip-html` |
| ⏰ 时间工具 | 日期计算器 | `/time/calculator` |
| 🎲 生成器 | 随机字符串 | `/generator/random-string` |
| | 密码生成器 | `/generator/password` |
| 🔧 其他工具 | 正则测试 | `/other/regex` |
| | 进制转换 | `/other/radix` |
| | 颜色转换 | `/other/color` |
| | 字符统计 | `/other/char-count` |

## 项目结构

```
src/
├── DevToolbox.Web/                 # Blazor WebAssembly 主项目
│   ├── Pages/                      # 工具页面组件
│   │   ├── Encoding/               #   编码/解码工具
│   │   ├── Crypto/                 #   加密/解密工具
│   │   ├── Formatter/              #   格式化工具
│   │   ├── Time/                   #   时间工具
│   │   └── Generator/              #   生成器工具
│   ├── Services/                   # 业务逻辑服务层
│   │   ├── EncodingService.cs      #   Base64/URL/HTML/Unicode/Hex 编解码
│   │   ├── CryptoService.cs        #   Hash + AES 加解密
│   │   ├── JwtService.cs           #   JWT Token 解析
│   │   ├── FormatterService.cs     #   JSON 格式化/压缩
│   │   ├── TimeService.cs          #   时间戳转换
│   │   ├── GeneratorService.cs     #   UUID/随机字符串/密码生成
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
| `IEncodingService` | 编码/解码 | Base64、URL、HTML、Unicode、Hex 编解码 |
| `ICryptoService` | 加密/哈希 | MD5/SHA 哈希、AES 加解密、随机密钥生成 |
| `IJwtService` | JWT 解析 | Token 解析、Claims 提取、过期检测 |
| `IFormatterService` | 格式化 | JSON 格式化/压缩 |
| `ITimeService` | 时间转换 | 时间戳 ↔ 日期互转 |
| `IGeneratorService` | 内容生成 | UUID、随机字符串、安全密码 |

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
- AES 密钥使用 `RandomNumberGenerator` 生成，密码学安全
