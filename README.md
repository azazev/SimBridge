# SimBridge

**An experimental Dalamud compatibility plugin connecting AnoMech simulations with BossMod Reborn encounter modules. Requires both plugins.**

Developed by **azazev**.

> **Release status:** v0.8.0-beta.1 is being staged privately. There is no published beta release or custom Dalamud repository URL yet.

## What it does

SimBridge lets supported BossMod Reborn encounter modules operate within selected AnoMech simulations. It reuses the upstream module's existing encounter guidance.

- Supports eight **Futures Rewritten (Ultimate)** simulations in the initial beta scope.
- Connects only when the corresponding simulation and supported BossMod module can be positively identified.
- Keeps unsupported BossMod modules out of simulations when its simulation-protection checks succeed.
- Recovers from supported dependency reload scenarios; reconnecting during a run requires a **fresh simulation**.
- Exposes an enable/disable setting, `/simbridge status`, and opt-in developer diagnostics.

**SimBridge does not add custom mechanic markers, new safe positions, strategy rules, or player movement automation.** It does not modify the installed source of either upstream plugin, and it does not change how BossMod behaves during real encounters.

## Requirements

- Final Fantasy XIV with Dalamud, compatible with **Dalamud API 15**
- [AnoMech](https://github.com/anomek/AnoMech)
- [BossMod Reborn](https://github.com/FFXIV-CombatReborn/BossmodReborn)

The verified live configuration for this development baseline was **AnoMech 0.5.1.0** and **BossMod Reborn 7.5.8.10**. Compatibility checks can accept other versions, but passing checks does not guarantee future behavior.

## Supported content

The first beta focuses on these FRU scenarios:

| Phase | Simulation |
| --- | --- |
| P2 | Diamond Dust, Light Rampant |
| P3 | Ultimate Relativity, Apocalypse |
| P4 | Darklit Dragonsong, Crystallize Time |
| P5 | Fulgent Blade, Paradise Regained |

This is a list of supported *integration scenarios*, not a claim of full strategy or role coverage. See [Compatibility and known limitations](COMPATIBILITY.md).

## Commands

- `/simbridge` — Open the controls.
- `/simbridge status` — Print integration/compatibility status.

Developer Mode makes diagnostic controls and the logs-folder shortcut available. Keep diagnostics disabled when you do not need them.

## Installation

Public installation instructions will be activated when a beta package and custom Dalamud repository are actually published. For the prepared instructions, see [INSTALLATION.md](INSTALLATION.md). **Do not use an unverified repository URL or rename the internal DLL yourself.**

## Reporting issues

Follow [TROUBLESHOOTING.md](TROUBLESHOOTING.md) before opening a GitHub Issue. Include dependency versions and exact steps; review logs before sharing them.

## AI-assisted development

AI-assisted tools were used to help develop and document SimBridge under human direction. The code has undergone automated checks and in-game validation, but that testing does not guarantee complete correctness.

## Credits and status

SimBridge is an **independent compatibility project**, not an official extension of AnoMech or BossMod Reborn. Thanks to the maintainers and contributors of both upstream projects.

All plugin functionality is intended to remain free. See [Third-party attribution notes](THIRD_PARTY_NOTICES.md). Licensing and any additional required notices are still under review **before public release**.

For the proposed beta changes, see [CHANGELOG.md](CHANGELOG.md).
