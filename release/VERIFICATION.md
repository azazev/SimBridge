# SimBridge v0.8.0-beta.1 — build verification

Date: 2026-11-24. Private staging; not a release announcement.

## Tested build

- Source commit: `0b011fb8bd1333154637ae0a422d2e80013bfb48`
- Windows .NET SDK 10.0.100, reference pack 10.0.0; Dalamud API 15.
- BossMod Reborn 7.5.8.10; AnoMech 0.5.1.0.
- Owner ran `./Build-SimBridge-v0.8.0.ps1 -BossModVersions '7.5.8.10'` from a fresh downloaded source checkout.
- Compilation passed with warnings treated as errors; all 10 offline test groups passed.
- All 11 installed compatibility groups passed (87 ms, offline reflection only).
- Historical .9 was not installed and was not retested in this run. Full historical test mode remains available.

The owner supplied console output, the validation log and SHA-256 results. Raw logs and local filesystem paths are deliberately excluded.

## Integrity

The original development archive was independently checked: SHA-256 `6b464968bd7241a991103a87c3385f319f7a3741ef16e9d8c32375616d55b194`. The three extracted original plugin files exactly match the hashes and sizes reported for the clean GitHub build.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| AnoBossBridge.dll | 189440 | 2ee6b505d985121a8c9f37c43eeec1b8ce32c36b909fa0a3710d8eff39aa5993 |
| AnoBossBridge.deps.json | 457 | 2bd145ab4e7c34da4dc5b080cf8bb3dc26ff995772bbc6424d17e77e05199c81 |
| AnoBossBridge.json | 645 | e307627c2253c59ea6989269d3e016156c86985a8d0bdb8e3460851c5676ec71 |

The runtime DLL is byte-identical to the previously live-tested v0.8.0 baseline. The clean checkout therefore demonstrated a matching rebuild in this environment, not fresh gameplay coverage, arbitrary future dependency compatibility, or a cross-platform reproducible build. Documentation/license files added after the tested commit do not change runtime source.

Release ZIP layout, license/corresponding-source inclusion, repository feed, installation/update/rollback and final publication approval are separate gates.
