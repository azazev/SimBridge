# Installation and updates

> **Not yet released.** These instructions are staged for review. As of this staging branch, there is **no live SimBridge custom repository URL, release download or public beta**.

## Requirements

Install and enable the current compatible versions of:
1. [Dalamud](https://dalamud.dev/)
2. [AnoMech](https://github.com/anomek/AnoMech)
3. [BossMod Reborn](https://github.com/FFXIV-CombatReborn/BossmodReborn)

The v0.8.0 baseline was tested live with BossMod Reborn **7.5.8.10**, AnoMech **0.5.1.0**, and Dalamud API **15**. Exact compatibility can change after updates; consult [COMPATIBILITY.md](COMPATIBILITY.md).

## Planned beta installation using a custom Dalamud repository

**These steps become actionable only after a public URL is announced in an official SimBridge release.**

1. Install and enable both upstream dependencies in Dalamud.
2. Open Dalamud settings and locate **Experimental → Custom Plugin Repositories**.
3. Add the verified **SimBridge custom repository URL from the release announcement**. (No URL is active yet.)
4. Open the Dalamud Plugin Installer; install **SimBridge** from that repository.
5. Open `/simbridge` and run `/simbridge status`. Confirm that the dependencies are visible and that the compatibility checks have passed.
6. Start a **fresh supported AnoMech simulation**.

Avoid copying a guessed `repo.json` URL or installing a second, manually loaded copy alongside the repository-managed plugin.

## Development installation (for designated testers only)

For a validated manual plugin package, keep its **three original matching files** together in a dedicated folder: `AnoBossBridge.dll`, `AnoBossBridge.json`, and `AnoBossBridge.deps.json`. Register the DLL using Dalamud's development-plugin location controls.

The internal `AnoBossBridge` name is intentionally preserved for configuration identity; **do not independently rename the DLL, JSON, or deps file**. These manual steps are not a substitute for a tested repository-delivery flow.

## Updating

Once a public custom repository is available, use Dalamud's plugin update mechanism. If a relevant upstream dependency is reloaded mid-simulation, end that run and start a new one; SimBridge does not resume halfway through.

## Uninstalling

Uninstall the repository-managed plugin through Dalamud, or remove the manually configured development-plugin location if that's how you installed it. Disable diagnostics before removal. Existing configuration and diagnostic files may remain; do not delete them if you want to preserve settings or evidence.

## Rollback

Previous internal v0.7.3 builds use exact dependency-version restrictions and do **not** accept BossMod Reborn 7.5.8.10. Use only a rollback build specifically tested with your installed dependency versions; do not assume an older ZIP will work.

Please consult [TROUBLESHOOTING.md](TROUBLESHOOTING.md) before reporting an installation issue.
