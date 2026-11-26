# Building and validating SimBridge

For the v0.9.0 candidate, run `./Build-SimBridge-v0.8.0.ps1 -BossModVersions @('7.5.8.10','7.5.8.11')` from PowerShell. The script retains its original filename for existing callers. The following release history also documents the original public beta.

The pinned Windows build has been verified from clean GitHub checkout `0b011fb8bd1333154637ae0a422d2e80013bfb48` on November 24, 2026. All ten offline groups and eleven installed compatibility groups passed with BossMod Reborn 7.5.8.10 and AnoMech 0.5.1.0. The DLL and two metadata files matched the tested development package by SHA-256. See [release/VERIFICATION.md](release/VERIFICATION.md). This is one verified environment, not a claim of universal reproducibility.

The existing direct Roslyn build pins .NET SDK 10.0.100, .NET reference pack 10.0.0 and Dalamud API 15. It preserves the tested runtime source, compile arguments, resource name and internal AnoBossBridge identity. No upstream binaries are bundled. This is a Windows/local-Dalamud build path, not a general cross-platform or hermetic build.

## Prerequisites

- Windows PowerShell or PowerShell on Windows.
- .NET SDK 10.0.100 and .NET 10 reference pack 10.0.0 under the selected DotnetRoot (default: Program Files\dotnet).
- For compilation: Dalamud API 15 development DLLs under %APPDATA%\XIVLauncher\addon\Hooks\dev (Dalamud.dll, Dalamud.Common.dll, Lumina.dll, Dalamud.Bindings.ImGui.dll). The owner-reported reference versions were Dalamud 15.0.4.1 and Lumina 7.7.0; see [release/PROVENANCE.md](release/PROVENANCE.md) for exact embedded revisions and SHA-256 fingerprints.
- For installed compatibility validation: AnoMech 0.5.1.0 and each selected BossMod Reborn version in the standard XIVLauncher\installedPlugins version folders under %APPDATA%, with their required assemblies and local Dalamud libraries available.

Do not downgrade a working game setup merely to run the historical suite. Use the explicit installed-version target when validating a current environment. The v0.9.0 default targets are 7.5.8.10 and 7.5.8.11. Both installed-assembly test groups receive the same list; other historical versions are not auto-discovered.

## Commands

Download or clone the reviewed source into a new folder. Open PowerShell in that folder; use only reviewed scripts. No administrator privileges are required.

Compile and run all ten offline groups, checking only BossMod 7.5.8.10 in both installed-assembly contract groups (6 and 10):

    .\Build-SimBridge-v0.8.0.ps1 -BossModVersions '7.5.8.10'

This copies the freshly compiled, version-checked DLL into plugin only after all selected offline checks succeed. The original checked-in manifest and dependency metadata are not rewritten.

Historical full validation (requires both 7.5.8.9 and 7.5.8.10):

    .\Build-SimBridge-v0.8.0.ps1

Compilation only, without tests or updating the plugin directory:

    .\Build-SimBridge-v0.8.0.ps1 -Mode BuildOnly

Offline validation only, without building or copying the plugin DLL (requires an existing DLL compiled from the same checkout; run BuildOnly first if needed):

    .\Build-SimBridge-v0.8.0.ps1 -Mode ValidateOnly -BossModVersions '7.5.8.10'

For a custom .NET installation, also pass -DotnetRoot with the root directory containing dotnet.exe, sdk and packs.

Missing selected versions, incorrect assembly versions, or failing contracts/tests are errors, not silently skipped tests. The final test output explicitly identifies the versions checked. A successful single-target run does not newly validate the other historical target. BuildOnly is not a release validation.

## Verify output

For the full build-and-validate command, inspect VALIDATION.txt (compiler/test output; final wrapper messages may appear only in the console), and confirm the three matching files in plugin:

    Get-ChildItem .\plugin | Select-Object Name, Length
    Get-FileHash .\plugin\AnoBossBridge.dll -Algorithm SHA256

Assembly and manifest version must be 0.8.0.0. Keep the internal DLL, manifest and deps names together; do not independently rename them. A fresh build hash may differ if compiler inputs differ; matching the tested DLL is not assumed. Compare against the validated archive and investigate differences before choosing the public asset.

Build/test outputs are local, ignored by Git. Do not commit them or diagnostic logs. This script does not change installed plugins. Do not load the new DLL until the release review has compared it with the tested baseline.

In-game tests, licensing and distribution verification are separate release gates.
