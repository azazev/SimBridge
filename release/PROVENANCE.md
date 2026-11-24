# Source provenance and release compliance review

Private staging record, 2026-11-24. This is an engineering review, not a legal opinion or a declaration that all licensing obligations are satisfied.

## Evidence reviewed

- Earlier development records identify source snapshots [AnoMech 1646a70](https://github.com/anomek/AnoMech/tree/1646a7068c2071a3d1fd9e3c84550dfd3f1b6ade) and [BossMod Reborn 35a908a](https://github.com/FFXIV-CombatReborn/BossmodReborn/tree/35a908ad71b1a94fdbf342518ed78c7c72c458c0), and the standard Dalamud framework/documentation. License files at those snapshots match current upstream license files at review time.
- Static review of source and build configuration shows a standalone C# bridge using reflection over local plugin assemblies, runtime integration, native callbacks and generated contract metadata; no third-party plugin binaries included in the three verified package files. Build and tests are recorded in [VERIFICATION.md](VERIFICATION.md).
- No explicit copied-code notice or permission record was found in the reviewed source and development records. That is not proof that no copied/adapted expressions exist. The records indicate upstream source was consulted during development.
- The owner selected AGPL-3.0-or-later for SimBridge's own contributions. This does not change upstream license terms. The BSD copyright and full license have been preserved; the upstream AGPLv3 text is included.
- The Free Software Foundation discusses plugin integration and corresponding source in its [GNU license FAQ](https://www.gnu.org/licenses/gpl-faq.html) and [AGPLv3](https://www.gnu.org/licenses/agpl-3.0.html), particularly sections 1, 5, 6 and 13. Close communication with upstream internals warrants review even though binaries are distributed separately.

## Additional review — owner confirmation and upstream release tags

- The owner confirmed that SimBridge is an independent personal project, with no other person, employer or company involved in its ownership or development; the owner approved AGPL-3.0-or-later for their own contributions and confirmed the preceding owner-side statement about no independently introduced external code outside the development sessions.
- AnoMech release [v0.5.1.0](https://github.com/anomek/AnoMech/releases/tag/v0.5.1.0) resolves to [6aec4a5940df56ebc4f70a2fdddeceb31c7848b8](https://github.com/anomek/AnoMech/tree/6aec4a5940df56ebc4f70a2fdddeceb31c7848b8). Its LICENSE.md is identical to the staged AGPLv3 text (Git blob 0ad25db4bd1d86c452db3f9602ccdbe172438f52).
- BossMod Reborn release [7.5.8.10](https://github.com/FFXIV-CombatReborn/BossmodReborn/releases/tag/7.5.8.10) resolves to [05bdf3abd1963c84105784273a8c171bb7cf0b7c](https://github.com/FFXIV-CombatReborn/BossmodReborn/tree/05bdf3abd1963c84105784273a8c171bb7cf0b7c). Its LICENSE is identical to the staged BSD-3-Clause text (Git blob 560bcd91c155c8c8a25d0df7a787c41e85c64bce). Its upstream third-party notices identify KamiToolKit (MIT) and RebornMaterial (LGPL-3.0-or-later); they are not bundled in the three SimBridge files.
- Published tag-to-source mapping is verified for these two upstream releases, not a cryptographic match of the locally installed DLLs to release assets. The exact locally used Dalamud/API 15 build and Lumina dependency revision still need identifying.
- A review of the original production source found no explicit copied/adapted-code notices; this does not establish absence of copied expression. No independent exhaustive comparison with all upstream or tutorial source has been performed.
- PR discussion retrieval returned zero comments at this review point.

## Open pre-publication gates

- Owner-side authorization is recorded. Identify any copied or adapted expressions (including AI-assisted contributions) and preserve all applicable notices; the owner confirmation does not certify the entire code provenance.
- The two upstream release tags and license files are resolved above; identify the precise Dalamud/API 15 and locally referenced Lumina build revisions and, where needed, match local binaries to upstream published assets.
- Assess combined-work and corresponding-source obligations for the intended binary distribution and preserve applicable upstream terms. Provide complete corresponding source, build/install scripts and any additional required source access for the exact release, rather than assuming a repository link alone satisfies every obligation.
- Verify license and notice files accompany distributed assets where required. The public release is blocked pending resolution.

## Privacy audit (initial)

Reachable main and staging branch history before this documentation commit: 14 commits, 55 complete file-change patches checked using patterns for emails, personal absolute paths, obvious keys/tokens and sensitive filenames. No matches found. All author identities used the azazev GitHub noreply address; the initial GitHub automated committer used noreply@github.com. No upstream binary or diagnostic log was found in the reviewed patches. The original verified DLL's printable ASCII and UTF-16LE strings were screened for these patterns; no matches or PDB filenames found.

This is a bounded static screening, not an exhaustive security or forensic audit. Tag listing was unavailable through the connector. Recheck final refs, PR discussions, final release archive, metadata and history after remaining edits; unreachable Git objects and external records were not audited.
