# Compatibility and known limitations

This document describes the **tested v0.8.0 development baseline** proposed for the first public beta. A supported scenario is not necessarily fully validated across every role, random outcome, or strategy.

## Verified environment

| Component | Version / evidence |
| --- | --- |
| SimBridge | 0.8.0.0 assembly (proposed release label `v0.8.0-beta.1`) |
| BossMod Reborn | **7.5.8.10** — live-validated on November 24, 2026 |
| BossMod Reborn | **7.5.8.9** — offline compatibility checks passed; not a complete live-validation claim |
| AnoMech | **0.5.1.0** — live-validated with BossMod 7.5.8.10 |
| Dalamud | API 15 / .NET 10 build baseline |

### Version resilience

SimBridge checks supported members and selected method behavior against **253 member contracts across 11 groups** and **39 targeted normalized method guards**, rather than relying only on an exact upstream version allowlist. The checks are evaluated on dependency changes, not every frame.

This is a compatibility safeguard, **not automatic support for every future update**. Changes in unguarded behavior or a new Dalamud API can still require investigation or a new release. When checks fail, the affected integration may be disabled or simulation protection may be unavailable. See `/simbridge status`.

## Supported FRU simulation integrations

| Encounter phase | Scenario |
| --- | --- |
| P2 | Diamond Dust |
| P2 | Light Rampant |
| P3 | Ultimate Relativity |
| P3 | Apocalypse |
| P4 | Darklit Dragonsong |
| P4 | Crystallize Time |
| P5 | Fulgent Blade |
| P5 | Paradise Regained |

BossMod's existing module decides what guidance is available. SimBridge does not add missing upstream mechanics or invent alternative player positions or automarkers.

## Known limitations

- **Diamond Dust:** one intermittent Frigid Stone AOE-versus-cone-bait guidance mismatch was observed during private installer testing (M2, NA · Partner Swap). It was not reproduced in two subsequent attempts after reinstalling. The cause is unconfirmed; reinstalling is not an established fix. Diagnostics did not capture sufficient per-target assignment detail to rule out a synchronization edge case. Follow the simulation's actual markers when guidance disagrees and report the variation. This remains an experimental-beta investigation item.
- **Crystallize Time:** tested combinations include selected Red Ice and Blue Darkness cases, but full role/debuff coverage and complete NAUR strategy parity remain unverified. An accepted rewind-position discrepancy exists. No additional markers are inserted to compensate.
- **Paradise Regained:** limited in-game validation; full variation coverage is unverified.
- **Other supported encounters:** selected configurations have been tested, but no exhaustive role/strategy certification is implied.
- **Unsupported simulations:** SimBridge tries to prevent non-supported BossMod modules from lingering **only when it can positively identify simulation actors and its protection checks succeed**. It cannot provide this protection when unloaded, disabled or incompatible.
- **Dependency reload mid-simulation:** after a relevant plugin reload, the bridge requires a new simulation and will not attach to the interrupted run midway through.
- **Native BossMod behavior:** incomplete or changed upstream hints remain upstream limitations; SimBridge does not substitute its own mechanics.
- **Real encounters:** the bridge's scenario restrictions apply only to identified AnoMech simulation actors and should not modify BossMod's normal real-fight guidance.

## Validation scope

The November 24 checkpoint reports **10/10 offline test groups** passing, plus focused live checks for supported startup/cleanup, unsupported-module isolation, idle dependency reload, in-run reload recovery, and Crystallize Time toggle/ownership cleanup.

These are the recorded checks on the specified environment, not a guarantee that every displayed visual marker or future dependency version is correct.

## Troubleshooting and reporting

Open `/simbridge status`; note the loaded plugin versions and the indicated compatibility groups. See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for a reproducible report template. Do not post logs publicly without checking for personal information.
