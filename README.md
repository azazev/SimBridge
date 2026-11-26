# SimBridge

**An experimental Dalamud compatibility plugin connecting AnoMech simulations with BossMod Reborn encounter modules. Requires both plugins.**

Developed by **azazev**.

[v0.8.0-beta.1](https://github.com/azazev/SimBridge/releases/tag/v0.8.0-beta.1) is available through the SimBridge custom Dalamud repository.

The private v0.9.0 candidate adds Ultimate Predation and Ultimate Suppression (UWU), and P5 Exaflares (UCoB), alongside the eight FRU simulations below. These three additions were tested with BossMod Reborn 7.5.8.11 and AnoMech 0.5.1.0.

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

Add this URL under **Dalamud Settings → Experimental → Custom Plugin Repositories**.

```text
https://raw.githubusercontent.com/azazev/SimBridge/main/repo.json
```

Install **SimBridge** by **azazev** from the plugin installer. See [Installation and updates](INSTALLATION.md) for the full steps.

## Reporting issues

Follow [TROUBLESHOOTING.md](TROUBLESHOOTING.md) before opening a GitHub Issue. Include dependency versions and exact steps; review logs before sharing them.

## AI-assisted development

AI-assisted tools were used to help develop and document SimBridge under human direction. The code has undergone automated checks and in-game validation, but that testing does not guarantee complete correctness.

## Credits and status

SimBridge is an **independent compatibility project**, not an official extension of AnoMech or BossMod Reborn. Thanks to the maintainers and contributors of both upstream projects.

## License

SimBridge's own copyrightable contributions, to the extent the project has the rights to license them, are offered under the **GNU Affero General Public License, version 3 or (at your option) any later version** (SPDX: AGPL-3.0-or-later). See [LICENSE](LICENSE). Third-party components retain their applicable licenses and copyright notices; see [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). SimBridge is provided without warranty as described in the license.

For source build instructions, see [BUILDING.md](BUILDING.md). The verified Windows build and file checksums are documented in [release/VERIFICATION.md](release/VERIFICATION.md).

## Support

All SimBridge features are free. If you would like to support development, optional donations are welcome on [Ko-fi](https://ko-fi.com/azazev). Donations are not required and do not unlock features or gameplay functionality.

For the beta changes, see [CHANGELOG.md](CHANGELOG.md).
