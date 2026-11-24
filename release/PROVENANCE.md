# Source provenance and release compliance review

Private staging record, 2026-11-24. This is an engineering review, not a legal opinion or a declaration that all licensing obligations are satisfied.

## Evidence reviewed

- Earlier development records identify source snapshots [AnoMech 1646a70](https://github.com/anomek/AnoMech/tree/1646a7068c2071a3d1fd9e3c84550dfd3f1b6ade) and [BossMod Reborn 35a908a](https://github.com/FFXIV-CombatReborn/BossmodReborn/tree/35a908ad71b1a94fdbf342518ed78c7c72c458c0), and the standard Dalamud framework/documentation. License files at those snapshots match current upstream license files at review time.
- Static review of source and build configuration shows a standalone C# bridge using reflection over local plugin assemblies, runtime integration, native callbacks and generated contract metadata; no third-party plugin binaries included in the three verified package files. Build and tests are recorded in [VERIFICATION.md](VERIFICATION.md).
- No explicit copied-code notice or permission record was found in the reviewed source and development records. That is not proof that no copied/adapted expressions exist. The records indicate upstream source was consulted during development.
- The owner selected AGPL-3.0-or-later for SimBridge's own contributions. This does not change upstream license terms. The BSD copyright and full license have been preserved; the upstream AGPLv3 text is included.
- The Free Software Foundation discusses plugin integration and corresponding source in its [GNU license FAQ](https://www.gnu.org/licenses/gpl-faq.html) and [AGPLv3](https://www.gnu.org/licenses/agpl-3.0.html), particularly sections 1, 5, 6 and 13. Close communication with upstream internals warrants review even though binaries are distributed separately.

## Open pre-publication gates

- Confirm the contributor has the rights to license SimBridge's own contributions, identify any code copied or adapted from other sources (including samples/tutorials), and preserve all applicable notices.
- Identify exact source revisions and licenses corresponding to installed AnoMech 0.5.1.0, BossMod Reborn 7.5.8.10 and relevant Dalamud/API 15 dependencies. Prior consulted snapshots are not a verified mapping of those installed builds.
- Assess combined-work and corresponding-source obligations for the intended binary distribution and preserve applicable upstream terms. Provide complete corresponding source, build/install scripts and any additional required source access for the exact release, rather than assuming a repository link alone satisfies every obligation.
- Verify license and notice files accompany distributed assets where required. The public release is blocked pending resolution.

## Privacy audit (initial)

Reachable main and staging branch history before this documentation commit: 14 commits, 55 complete file-change patches checked using patterns for emails, personal absolute paths, obvious keys/tokens and sensitive filenames. No matches found. All author identities used the azazev GitHub noreply address; the initial GitHub automated committer used noreply@github.com. No upstream binary or diagnostic log was found in the reviewed patches. The original verified DLL's printable ASCII and UTF-16LE strings were screened for these patterns; no matches or PDB filenames found.

This is a bounded static screening, not an exhaustive security or forensic audit. Tag listing was unavailable through the connector. Recheck final refs, PR discussions, final release archive, metadata and history after remaining edits; unreachable Git objects and external records were not audited.
