# Third-party projects and licensing review

> **Draft for private staging.** This is not a license file. Attribution and license compliance must be reviewed before SimBridge's source or binary becomes public.

## Upstream dependencies

- [AnoMech](https://github.com/anomek/AnoMech) — FFXIV mechanics simulator, credited to its maintainers and contributors. Its upstream repository identifies an AGPL-3.0-family license.
- [BossMod Reborn](https://github.com/FFXIV-CombatReborn/BossmodReborn) — encounter modules and mechanic guidance, credited to its maintainers and contributors. Its upstream repository identifies BSD-3-Clause licensing.

SimBridge is a separate community compatibility project by **azazev**. It is not endorsed by or officially affiliated with either dependency.

## Before public distribution

- Determine whether any SimBridge code reproduces or derives from upstream code and what license conditions apply.
- Preserve attribution, copyright notices and any licenses required for redistributed components.
- Confirm whether release archives include any upstream binaries, code, or assets; the reviewed v0.8.0 development package did not bundle upstream plugin DLLs.
- Select and publish the appropriate SimBridge license and final third-party notices **before the repository becomes public**.

Do not interpret this draft as granting permission to reuse the code or as a final legal conclusion.
