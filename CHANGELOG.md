# Changelog

## [0.8.0-beta.1] — proposed, unpublished

**Status:** Private release preparation. This label does not imply a GitHub Release, repository feed, or tag already exists.

The proposed first beta is based on the **v0.8.0 development baseline** validated on November 24, 2026.

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
- Full role/strategy coverage, particularly Crystallize Time and Paradise Regained, is **not** claimed.

### Packaging

The compiled baseline is `0.8.0.0` and retains the legacy `AnoBossBridge` internal file identity to preserve settings and logs. A beta label in documentation does not imply a changed DLL assembly version.

## Earlier builds

Versions **0.7.x and earlier** were internal development checkpoints, not previously published public releases. Their detailed changelogs are not provided here; do not assume release compatibility or rollback support.
