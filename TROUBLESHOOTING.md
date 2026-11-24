# Troubleshooting and bug reports

SimBridge is experimental. Confirm that **AnoMech** and **BossMod Reborn** are installed and enabled before investigating an integration issue.

## Quick checks

| Symptom | What to check |
| --- | --- |
| `/simbridge` does not open | Confirm SimBridge is loaded in Dalamud; ensure there isn't a second development copy |
| No BossMod guidance in a simulation | Confirm the encounter is on [the supported list](COMPATIBILITY.md), check `/simbridge status`, and try a fresh simulation |
| Compatibility warning | Record the versions and exact failed contract group; an upstream update may require review |
| Reloaded BossMod during a run | Exit the simulation and start a **new** attempt; mid-run reattachment is intentionally blocked |
| Unsupported simulation has no module | Expected when the simulation-only protection is active; non-supported modules are intentionally blocked |
| Protection is unavailable | SimBridge can only protect identified simulations when loaded, enabled and its protection checks pass |
| Stale display after exiting | End the simulation, run `/simbridge status`, and capture diagnostics if a module remains |
| Incorrect mechanic guidance | Confirm role, scenario, strategy and whether native BossMod supports that variation; SimBridge does not add extra markers |

## Diagnostics

1. Open `/simbridge` or run `/simbridge status`.
2. Enable **Developer Mode** to expose diagnostic controls.
3. Start diagnostics **before reproducing** a problem when possible.
4. Reproduce it in a supported simulation; record the approximate point in the timeline.
5. Stop diagnostics and use **Open logs folder**.
6. Review all files for character names, unique identifiers or other personal details **before sharing** them.

Do not upload full logs automatically or post private information in public issue threads.

## Suggested GitHub issue report

**Title:** [Scenario] short description

- SimBridge build/version:
- BossMod Reborn version:
- AnoMech version:
- Dalamud API version:
- Scenario and phase:
- Role, strategy, relevant debuffs/settings:
- Steps to reproduce:
- Expected behavior:
- Actual behavior:
- Did it occur on a fresh simulation?:
- Did `/simbridge status` report incompatibilities?:
- Sanitized diagnostic log or status snapshot (optional):

## Unsupported mechanics

Only the eight scenarios listed in [COMPATIBILITY.md](COMPATIBILITY.md) are currently integration targets. Unsupported AnoMech simulations should not trigger BossMod modules through SimBridge.

## Privacy

Diagnostic output may include runtime state and identifiers. Read it before sharing it with anyone, even when submitting a private bug report.
