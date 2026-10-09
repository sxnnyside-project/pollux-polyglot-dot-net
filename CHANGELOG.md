# Changelog

All notable changes to **Pollux Polyglot .NET** are documented here.

This project follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [0.1.0] — 2026-10-09

### Added

- Initial release of `Sxnnyside.Pollux` for the .NET Runtime (`net10.0` and `net8.0`).
- Windows-first dynamic library resolution and cross-platform native loading via `LibraryLoader`.
- Cross-language interop for C#, Visual Basic .NET (VB.NET), and F#.
- Memory-safe managed wrapper over `pollux-abi/1` native engine instances.
- Deterministic capability evaluation via `PolluxEngine.Evaluate` and `PolluxEngine.EvaluateAsync`.
- JSON-backed operation models with snake_case serialization mapping.
- Resource lifecycle management implementing `IDisposable` and `IAsyncDisposable`.
- Comprehensive xUnit test suite for status verification, verdicts, and finalization safety.

---

[Unreleased]: https://github.com/sxnnyside-project/pollux-polyglot-dot-net/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/sxnnyside-project/pollux-polyglot-dot-net/releases/tag/v0.1.0
