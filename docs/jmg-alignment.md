# JMG → NMG 功能对标分析

## JMG (Java Memory-shell Generator) 完整功能清单

### 协议支持 (7 种)

| # | 协议 | 对接工具 | JMG 类 | NMG 现状 | NMG 需要新增 |
|---|------|---------|--------|---------|-------------|
| 1 | Godzilla | 哥斯拉 4.x | jmg/godzilla/* (7 个 memshell 类) | ✅ 已实现 | — |
| 2 | Behinder | 冰蝎 4.x | jmg/behinder/* (6 个) | ✅ 已实现 | — |
| 3 | AntSword | 蚁剑 | jmg/antsword/* (5 个) | ✅ 已实现 | — |
| 4 | Suo5 | Suo5 隧道 | jmg/suo5/* (5 个) | ❌ | Suo5Filter/Listener/Interceptor |
| 5 | Neo-reGeorg | Neo-reGeorg 隧道 | jmg/neoregeorg/* (5 个) | ❌ | NeoreGeorgFilter/Listener/Interceptor |
| 6 | Custom | 自定义 | jmg/custom/* | ❌ | CustomGenerator |
| 7 | — | — | — | — | — |

### 中间件模板 (16 种 Java 中间件)

| # | 中间件 | 注入类型 | JMG 类 | .NET 对应目标 | NMG 现状 |
|---|--------|---------|--------|-------------|---------|
| 1 | Tomcat | Filter/Listener/Valve/Agent | 4 类 | IIS Integrated Mode | ✅ VPP |
| 2 | SpringMVC | Interceptor/Agent | 2 类 | ASP.NET MVC Filters | ❌ |
| 3 | Spring WebFlux | HandlerMethod | 1 类 | — | N/A |
| 4 | Jetty | Filter/Listener | 2 类 | — | N/A |
| 5 | Resin | Filter/Listener | 2 类 | — | N/A |
| 6 | Undertow | Filter/Listener | 2 类 | — | N/A |
| 7 | WebLogic | Filter/Listener/Agent | 3 类 | — | N/A |
| 8 | WebSphere | Filter/Listener | 2 类 | — | N/A |
| 9 | WildFly | Filter/Listener | 2 类 | — | N/A |
| 10 | GlassFish | Filter/Listener | 2 类 | — | N/A |
| 11 | Apusic | Filter/Listener | 2 类 | 金蝶 Apusic（国产） | — |
| 12 | BES | Filter/Listener | 2 类 | 宝兰德 BES（国产） | — |
| 13 | InforSuite | Filter/Listener | 2 类 | 中创 InforSuite（国产） | — |
| 14 | TongWeb | Listener | 1 类 | 东方通 TongWeb（国产） | — |

### 输出格式 (7 种)

| # | 格式 | JMG 类 | NMG 现状 | 用途 |
|---|------|--------|---------|------|
| 1 | JSP | JSPFormater | ❌ (N/A, .NET 用 ASPX) | JSP webshell |
| 2 | JAR | JARFormater | ✅ dll (等效) | 二进制文件 |
| 3 | JAR-Agent | JARAgentFormater | ❌ | Java Agent 注入 |
| 4 | BASE64 | BASE64Formater | ✅ b64 | Assembly.Load 投递 |
| 5 | BCEL | BCELFormater + BCELoader | ❌ | Java 类加载器注入 |
| 6 | BigInteger | BigIntegerFormater | ❌ | 大整数编码绕过 |
| 7 | JavaScript | JavaScriptFormater | ❌ | JS 引擎注入 |

### 工具类

| 工具 | JMG 类 | 功能 | NMG 等效 |
|------|--------|------|---------|
| JavassistUtil | ✅ | 运行时字节码生成 | RoslynCompiler |
| InjectorUtil | ✅ | 注入器核心逻辑 | Templates/*.cs |
| JDKBypassUtil | ✅ | JDK 模块系统绕过 | N/A (.NET 无模块系统) |
| RandomHttpHeaderUtil | ✅ | 随机 HTTP 头生成 | SeedRandom + trigger name |
| ClassNameUtil | ✅ | 类名混淆 | SeedRandom.NextClassName |
| PackageNameUtil | ✅ | 包名混淆 | SeedRandom.NextNamespace |
| ResponseUtil | ✅ | 响应格式化 | GodzillaCSharp.TriggerBlock |
| CtClassUtil | ✅ | CtClass 操作 | Roslyn SyntaxTree |

---

## NMG (.NET) 功能对标规划

### .NET 生态的"中间件"对应关系

JMG 面向 Java 中间件，NMG 面向 .NET 运行时。映射关系：

| Java 中间件注入类型 | .NET 等效注入类型 | NMG 状态 |
|-------------------|-----------------|---------|
| Tomcat Filter | IHttpModule | ❌ 需新增 |
| Tomcat Listener | Global.asax Application_Start | ❌ 需新增 |
| Tomcat Valve | IHttpHandler | ❌ 需新增 |
| SpringMVC Interceptor | MVC ActionFilter | ❌ 需新增 |
| Spring WebFlux | ASP.NET Core Middleware | ❌ Phase 3 |
| Servlet | ASP.NET Page/IHttpHandler | ✅ VPP (等效) |
| Agent/Instrumentation | Assembly.Load + CLR Profiler | ✅ b64 (等效) |
| — | RouteBase (MVC/WebAPI) | ✅ Route |
| — | HttpListener (独立) | ✅ HttpListener |
| — | VirtualPathProvider | ✅ VPP |

### NMG 需要新增的功能（按优先级）

#### P0 — 核心缺失（影响基本可用性）

| # | 功能 | 说明 | 预估工作量 |
|---|------|------|-----------|
| 1 | HttpModule 型 | IIS 管道注入，所有 ASP.NET 请求都经过 | 2-3 天（需解决集成模式注册时机问题） |
| 2 | Global.asax 型 | Application_Start 注入，最经典的 .NET 内存马 | 1-2 天 |
| 3 | Suo5 协议 | HTTP 隧道，红队必备 | 1-2 天 |
| 4 | Neo-reGeorg 协议 | HTTP 隧道，红队必备 | 1-2 天 |

#### P1 — 对标 JMG

| # | 功能 | 说明 | 预估工作量 |
|---|------|------|-----------|
| 5 | MVC Filter 型 | ASP.NET MVC ActionFilter 注入 | 1 天 |
| 6 | WebAPI DelegatingHandler | ASP.NET WebAPI 管道注入 | 1 天 |
| 7 | Custom 协议 | 自定义命令协议，不依赖第三方工具 | 1 天 |
| 8 | JavaScript 输出格式 | JScript.Encode / JS 引擎注入 | 0.5 天 |
| 9 | BigInteger 输出格式 | 大整数编码绕过 WAF | 0.5 天 |

#### P2 — 增强

| # | 功能 | 说明 |
|---|------|------|
| 10 | 环境自动探测增强 | 自动检测 IIS 模式/MVC 版本/trust level 并推荐最佳注入方式 |
| 11 | 批量生成 | 一次生成多种类型×多种协议的组合 |
| 12 | 混淆等级选择 | 低/中/高三级混淆强度 |
| 13 | ViewState gadget | ViewState 反序列化注入（无文件直达） |

### 最终目标矩阵（.NET 版 JMG）

```
NMG Complete Matrix:

Protocols (7):     Godzilla ✅ | Behinder ✅ | AntSword ✅ | Suo5 ❌ | Neo-reGeorg ❌ | Custom ❌ | RawCmd ❌
MemoryShell (9):   VPP ✅ | Route ✅ | HttpListener ✅ | HttpModule ❌ | GlobalAsax ❌ | MVCFilter ❌ | WebAPI ❌ | HttpHandler ❌ | ASP.NET Core ❌
Output (5):        aspx ✅ | b64 ✅ | dll ✅ | JavaScript ❌ | BigInteger ❌

Current: 3 types × 3 protocols × 3 formats = 27 combinations
Target:  9 types × 7 protocols × 5 formats = 315 combinations
```
