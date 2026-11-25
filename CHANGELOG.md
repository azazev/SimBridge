# Changelog

## [0.8.0-beta.1](https://github.com/azazev/SimBridge/releases/tag/v0.8.0-beta.1) — November 25, 2026

The first public beta is based on the **v0.8.0 development baseline** validated on November 24, 2026.

### Changes compared with earlier internal builds

- Added a resilience layer that checks upstream compatibility by 253 member contracts across 11 independent groups and 39 targeted method guards.
- Improved recovery when BossMod Reborn or AnoMech disappears or reloads.
- Requires a fresh simulation after dependency replacement during an active attempt.
- Preserves module ownership and retries cleanup after certain failures.
- Keeps supported scenario activation separate from the simulation-only unsupported-module lock.
- Retains existing BossMod role assignments and native guidance; no new mechanic markers, new safe positions, altered strategies or player movement automation.
- Continues to expose `/simbridge`, `/simbridge status`, a saved enable preference and opt-in Developer Mode diagnostics.

### Supported beta scope

Eight FRU simulation integrations: Diamond Dust, Light Rampant, Ultimate Relativity, Apocalypse, Darklit Dragonsong, Crystallize Time, Fulgent Blade, and Paradise Regained.

### Validation

- Ten focused offline test groups passed.
- Live lifecycle and recovery validation completed on BossMod Reborn **7.5.8.10**, AnoMech **0.5.1.0**, Dalamud API **15**.
- Private custom-repository delivery, installer-based loading, configuration persistence and same-version uninstall/reinstall passed on the tested Windows client.
- Diamond Dust smoke testing passed for the tested attempts; one intermittent AOE-versus-cone-bait mismatch was observed and was not reproduced in two subsequent attempts. The cause remains unresolved. See [COMPATIBILITY.md](COMPATIBILITY.md).
- Full role/strategy coverage, particularly Crystallize Time and Paradise Regained, is **not** claimed. A deliberately failed attempt was counted as a functional/lifecycle test, not a successful mechanic clear.

### Packaging

The compiled baseline is `0.8.0.0` and retains the legacy `AnoBossBridge` internal file identity to preserve settings and logs. A beta label in documentation does not imply a changed DLL assembly version.

## Earlier builds

Versions **0.7.x and earlier** were internal development checkpoints, not previously published public releases. Their detailed changelogs are not provided here; do not assume release compatibility or rollback support.
