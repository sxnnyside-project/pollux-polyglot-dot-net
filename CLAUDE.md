# CLAUDE.md — Pollux Polyglot .NET

> Context and development guidelines for humans and AI agents working on `pollux-polyglot-dot-net`.

## Overview

`pollux-polyglot-dot-net` is the official .NET language binding for the **Pollux Core** deterministic authority sandboxing engine (`pollux-abi/1`). It is designed with a **Windows-first** architecture while supporting complete cross-platform fidelity on Linux and macOS.

It exposes a high-level, memory-safe API for applications built on **C#**, **Visual Basic .NET (VB.NET)**, and **F#**.

## Commands

All lifecycle workflows are managed through [`just`](https://github.com/casey/just) or directly with the .NET CLI:

| Command | Action |
| :--- | :--- |
| `just install` | Restore all NuGet dependencies (`dotnet restore`) |
| `just dev` | Build solution in Debug configuration (`dotnet build`) |
| `just build` | Build solution in Release configuration (`dotnet build -c Release`) |
| `just test` | Run xUnit test suite (`dotnet test`) |
| `just check` | Full non-mutating quality gate (`dotnet build && dotnet test`) |
| `just lint` | Verify formatting without modifications (`dotnet format --verify-no-changes`) |
| `just format` | Format source code (`dotnet format`) |
| `just pack` | Create NuGet package (`dotnet pack -c Release -o ./artifacts`) |
| `just clean` | Remove build caches and binary outputs |

## Architecture & Code Conventions

1. **Language & Frameworks**:
   - Primary library targets `net10.0` and `net8.0` via `<TargetFrameworks>`.
   - Test suite runs on modern `net10.0`.
   - Strict nullable reference types (`<Nullable>enable</Nullable>`).
   - Treat warnings as errors enabled across the solution.
2. **Naming & Layout**:
   - `src/Pollux/`: Main library project `Sxnnyside.Pollux`.
   - `tests/Pollux.Tests/`: xUnit test project `Sxnnyside.Pollux.Tests`.
   - Types, properties, and methods follow standard .NET `PascalCase`.
   - JSON serialized properties match Rust ABI snake_case (configured via `[JsonPropertyName("...")]`).
3. **Memory Safety & Interop**:
   - Wraps unmanaged pointers deterministically inside `PolluxEngine`.
   - Exposes both `IDisposable` and `IAsyncDisposable` with safe finalizer suppression (`GC.SuppressFinalize`).
   - All native calls occur through safe delegates initialized via `NativeLibrary.Load` and `Marshal.GetDelegateForFunctionPointer`.
   - Dynamic library discovery is handled by `Internal/LibraryLoader`, prioritizing Windows paths and runtime identifiers (`runtimes/{rid}/native/`).

## Verification Prior to Commit

Always ensure that `just check` succeeds cleanly with **0 warnings and 0 errors** before pushing or opening a pull request.
