# Pollux Polyglot .NET

![Version](https://img.shields.io/badge/version-0.1.0-blue)
![License](https://img.shields.io/badge/License-MIT-green)
[![CI](https://github.com/sxnnyside-project/pollux-polyglot-dot-net/workflows/CI/badge.svg)](https://github.com/sxnnyside-project/pollux-polyglot-dot-net/actions)
[![NuGet](https://img.shields.io/nuget/v/Sxnnyside.Pollux.svg)](https://www.nuget.org/packages/Sxnnyside.Pollux/)

<p align="center">
  <strong>Universal .NET ✦ Windows-Focused ✦ Deterministic Authority</strong><br>
  <em>Deterministic execution authority and sandboxing SDK for the .NET Runtime across C#, Visual Basic .NET, and F#.</em>
</p>

<p align="center">
  <a href="#about">About</a> ✦
  <a href="#features">Features</a> ✦
  <a href="#installation">Installation</a> ✦
  <a href="#usage">Usage</a> ✦
  <a href="#architecture">Architecture</a> ✦
  <a href="#contributing">Contributing</a>
</p>

---

## About

**Pollux Polyglot .NET** is the official .NET language binding for the Pollux Core deterministic authority sandboxing engine.

Engineered with a Windows-first architecture and complete cross-platform fidelity across Linux and macOS, it enables applications running on the .NET Runtime (`net10.0` and `net8.0`) to interoperate with the native Rust Core (`pollux-abi/1`).

Whether you are authoring enterprise backends in C#, legacy business systems or desktop tooling in Visual Basic .NET (VB.NET), or functional pipelines in F#, Pollux enforces fine-grained Authority Manifests, deterministic sandboxing, and cryptographically verified capability delegation.

### Philosophy

> *"Deterministic capability enforcement for the modern .NET ecosystem with zero raw pointer leaks and native Windows fidelity."*

This is a Sxnnyside project, part of the Sxnnyside Project's core ecosystem.

## Features

- **Windows-First Optimization**: Native support for Windows DLL resolution, standard Windows directories (`ProgramFiles`, `LocalAppData`, `System32`), and environment path traversal.
- **Cross-Language .NET Interop**: Idiomatic APIs designed for seamless consumption in C#, Visual Basic .NET (VB.NET), and F#.
- **Modern .NET Lifecycle**: Full implementation of `IDisposable` and `IAsyncDisposable` for non-blocking asynchronous resource finalization.
- **Dual Target Frameworks**: Built for modern .NET 10 while maintaining LTS enterprise compatibility with .NET 8.
- **Zero Raw Pointer Leaks**: Memory-safe wrappers prevent unsafe unmanaged pointers from escaping into user code.
- **Deterministic Evaluation**: Instant JSON-backed verdicts (`allow`, `deny`, `escalate`, `trace`) governed by capability manifests.
- **ABI Version Verification**: Automatic handshake and validation against `pollux-abi/1`.

## Installation

### Prerequisites

- .NET SDK (>= 8.0, 10.0 recommended)

### Package Manager

Install the official package from NuGet:

```bash
dotnet add package Sxnnyside.Pollux
```

Or add the dependency directly to your project file (`.csproj`, `.vbproj`, `.fsproj`):

```xml
<PackageReference Include="Sxnnyside.Pollux" Version="0.1.0" />
```

### From Source

```bash
git clone https://github.com/sxnnyside-project/pollux-polyglot-dot-net.git
cd pollux-polyglot-dot-net

just install
just dev
```

## Usage

### C# Example

```csharp
using Sxnnyside.Pollux;

// Initialize engine with an Authority Manifest
string manifestJson = """
{
  "version": 1,
  "identity": "dot-net-worker",
  "capabilities": ["fs:read", "net:connect"]
}
""";

await using var engine = new PolluxEngine(manifestJson);

// Verify engine status
if (engine.Status != PolluxStatus.Valid)
{
    Console.WriteLine($"Engine initialization error: {engine.LastError}");
    return;
}

// Request permission for an operation
var operation = new Operation
{
    Kind = "fs:read",
    Resource = "C:\\Windows\\System32\\drivers\\etc\\hosts",
    Principal = "dot-net-worker"
};

EvaluationResult result = await engine.EvaluateAsync(operation);

Console.WriteLine($"Verdict: {result.Verdict}");
Console.WriteLine($"Allowed: {result.IsAllowed}");
Console.WriteLine($"Signature: {result.Signature}");
```

### Visual Basic .NET (VB.NET) Example

```vb
Imports System
Imports System.Threading.Tasks
Imports Sxnnyside.Pollux

Module Program
    Sub Main()
        MainAsync().GetAwaiter().GetResult()
    End Sub

    Async Function MainAsync() As Task
        Dim manifestJson As String = "{""version"": 1, ""identity"": ""vb-service"", ""capabilities"": [""fs:read""]}"

        Await Using engine As New PolluxEngine(manifestJson)
            If engine.Status <> PolluxStatus.Valid Then
                Console.WriteLine($"Engine error: {engine.LastError}")
                Return
            End If

            Dim op As New Operation With {
                .Kind = "fs:read",
                .Resource = "C:\Data\finance.xlsx",
                .Principal = "vb-service"
            }

            Dim result As EvaluationResult = Await engine.EvaluateAsync(op)
            Console.WriteLine($"Verdict: {result.Verdict} (Allowed: {result.IsAllowed})")
        End Using
    End Function
End Module
```

### F# Example

```fsharp
open System
open System.Threading.Tasks
open Sxnnyside.Pollux

let evaluateOperation () =
    task {
        let manifest = """{"version": 1, "identity": "fsharp-agent", "capabilities": ["net:connect"]}"""
        use engine = new PolluxEngine(manifest)
        
        let op = Operation(Kind = "net:connect", Resource = "api.sxnnysideproject.com", Principal = "fsharp-agent")
        let! result = engine.EvaluateAsync(op)
        
        printfn "Verdict: %s, Allowed: %b" result.Verdict result.IsAllowed
    }
```

## Architecture

```
pollux-polyglot-dot-net/
├── src/         # Main library implementation (Sxnnyside.Pollux)
├── tests/       # Test suite and integration verifications (Sxnnyside.Pollux.Tests)
└── scripts/     # Development and CI validation scripts
```

```
┌────────────────────────────────────────────────────────┐
│               .NET Application Layer                  │
│       C#    ✦    Visual Basic .NET    ✦    F#         │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│                    Sxnnyside.Pollux                    │
│   • PolluxEngine (IDisposable, IAsyncDisposable)       │
│   • Operation, EvaluationResult, PolluxStatus          │
│   • Internal LibraryLoader (Windows / Linux / macOS)   │
└───────────────────────────┬────────────────────────────┘
                            │ NativeLibrary.Load
┌───────────────────────────▼────────────────────────────┐
│                 Native Dynamic Library                 │
│    Windows: pollux_ffi.dll                             │
│    macOS:   libpollux_ffi.dylib                        │
│    Linux:   libpollux_ffi.so                           │
└───────────────────────────┬────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────┐
│                  Pollux Core (Rust)                    │
│                 ABI: pollux-abi/1                      │
└───────────────────────────┬────────────────────────────┘
```

## Contributing

Contributions are accepted. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

Before contributing, read the [Code of Conduct](CODE_OF_CONDUCT.md).

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

<p align="center">
  <strong>Pollux Polyglot .NET</strong> — A Sxnnyside Project<br>
  <em>&copy; 2026 Sxnnyside Project</em>
</p>
