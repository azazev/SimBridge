# Installation and updates

[SimBridge v0.9.0](https://github.com/azazev/SimBridge/releases/tag/v0.9.0) is available through the project's custom Dalamud repository. Dalamud displays the assembly version as **0.9.0.0**.

## Requirements

Install and enable the current compatible versions of:
1. [Dalamud](https://dalamud.dev/)
2. [AnoMech](https://github.com/anomek/AnoMech)
3. [BossMod Reborn](https://github.com/FFXIV-CombatReborn/BossmodReborn)

The v0.9.0 development baseline was tested live with BossMod Reborn **7.5.8.11**, AnoMech **0.5.1.0**, and Dalamud API **15**. Exact compatibility can change after updates; consult [COMPATIBILITY.md](COMPATIBILITY.md).

## Install the public beta

1. Enable AnoMech and BossMod Reborn. If you previously tested SimBridge, back up your settings, remove its development-plugin registration or old local repository, and uninstall any old installer copy. Keep the saved configuration.
2. Open Dalamud Settings (`/xlsettings`) and go to **Experimental → Custom Plugin Repositories**.
3. Paste this URL, click **+**, enable the entry and save.

   ```text
   https://raw.githubusercontent.com/azazev/SimBridge/main/repo.json
   ```

4. Open the Dalamud Plugin Installer (`/xlplugins`), refresh the list and install **SimBridge** by **azazev**. Check that it is enabled.
5. Open `/simbridge` and run `/simbridge status`. Check that the dependencies are detected and review the compatibility results.
6. With the bridge enabled, start a fresh [supported simulation](COMPATIBILITY.md).

## Development installation (for designated testers only)

For a validated manual plugin package, keep its **three original matching files** together in a dedicated folder: `AnoBossBridge.dll`, `AnoBossBridge.json`, and `AnoBossBridge.deps.json`. Register the DLL using Dalamud's development-plugin location controls.

The internal `AnoBossBridge` name is intentionally preserved for configuration identity; **do not independently rename the DLL, JSON, or deps file**. These manual steps are not a substitute for a tested repository-delivery flow.

## Updating

Use Dalamud’s plugin update mechanism. If an upstream dependency reloads during a simulation, finish or exit that run and start a fresh one. The private installer test verified an in-place update from 0.8.0.0 to 0.9.0.0 and a rollback by reinstalling 0.8.0.0 with the saved configuration.

## Uninstalling

Uninstall the repository-managed plugin through Dalamud, or remove the manually configured development-plugin location if that's how you installed it. Disable diagnostics before removal. Existing configuration and diagnostic files may remain; do not delete them if you want to preserve settings or evidence.

## Rollback

Previous internal v0.7.3 builds use exact dependency-version restrictions and do **not** accept BossMod Reborn 7.5.8.10. Use only a rollback build specifically tested with your installed dependency versions; do not assume an older ZIP will work.

Please consult [TROUBLESHOOTING.md](TROUBLESHOOTING.md) before reporting an installation issue.
