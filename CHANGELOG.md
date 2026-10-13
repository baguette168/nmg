# Changelog

## v0.2.0 (2026-10-14)

### Added
- AntSword protocol support (Base64 command execution)
- `probe` subcommand for environment detection aspx generation
- Web UI with Inset Dark theme (ASP.NET Core minimal API)
- Route memory shell template (RouteBase for MVC/WebAPI)
- HttpListener memory shell template (standalone, SYSTEM-level)
- GitHub Actions CI (build + test + publish + auto-release)
- 55 unit tests (protocol templates, compiler, formats, integration)

### Verified
- Godzilla protocol end-to-end (AES-128 encrypted response)
- Behinder protocol end-to-end (raw AES byte communication)
- AntSword protocol end-to-end (base64 command/response)
- VPP registration with dual-method fallback (official API + direct field replacement)
- Full red team attack chain (upload → inject → trigger → delete → connect)
- Stability: 5/5 cycles without failure
- Deterministic output: same seed produces byte-identical DLL

### Changed
- HttpListener restricted to Godzilla protocol only
- RoslynCompiler: deterministic compilation enabled, fixed assembly name

## v0.1.0 (2026-10-14)

### Added
- CLI: `nmg generate` with --type/--shell/--pass/--seed/--target/--format
- Godzilla C# protocol payload (AES-128, key=MD5(pass+key)[0:16])
- VPP registrar template with dual registration method
- Roslyn compilation engine targeting net40/net472
- Randomizer: namespace/class/path randomization with deterministic seed
- Output formats: aspx injector page + raw Base64
- Connection info quick-reference output
- 10 unit tests
