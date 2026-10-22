# Changelog

All notable changes to this package are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this package follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html). Each release is tagged `MAJOR.MINOR.PATCH` (no `v` prefix), matching the `version` in `package.json`.

Releases and changelog entries are generated automatically from [Conventional Commits](https://www.conventionalcommits.org/) via `semantic-release`. `fix:` → patch, `feat:` → minor. A major bump is never automatic — even a breaking-change commit only triggers a minor release — so going to the next major version is a deliberate action: push the next major version as a tag yourself (e.g. `3.0.0`), and subsequent automated releases build on top of it.

## [Unreleased]

## [0.1.0] - 2026-10-22

### Added

- ArchUnitNET (Mono.Cecil-based) bundled in `Plugins/` for use with Unity's test framework.
- `Editor/ArchUnitAssert.cs` — `Check`/`CheckAll` extensions that fail an NUnit test with ArchUnitNET's violation report.
- `Editor/AsmdefAssemblyLoader.cs` — resolves an `AssemblyDefinitionAsset` or asmdef path to its compiled `Assembly`.
- `Editor/ArchUnitPackages.cs` — `Package("...")` helper resolving a package's `Runtime`, `Editor`, `Tests`, `TestsEditor`, `TestsRuntime` assemblies by Unity's folder convention.
- EditMode tests covering rule evaluation and both helpers, including a dependency-direction check between this package's own Editor and Tests assemblies.
- MIT license and third-party notices for ArchUnit and ArchUnitNET.
- CI: EditMode tests run via GameCI against Unity 2021.3 LTS.

[Unreleased]: https://github.com/HamerSoft/ArchUnit-Unity/compare/0.1.0...HEAD
[0.1.0]: https://github.com/HamerSoft/ArchUnit-Unity/releases/tag/0.1.0
