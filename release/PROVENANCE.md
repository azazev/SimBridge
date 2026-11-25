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
- Published tag-to-source mapping is verified for these two upstream releases, not a cryptographic match of the locally installed DLLs to release assets. The local Dalamud/API 15 and Lumina development reference revisions were subsequently identified from owner-provided assembly product versions; see below.
- A review of the original production source found no explicit copied/adapted-code notices; this does not establish absence of copied expression. No independent exhaustive comparison with all upstream or tutorial source has been performed.
- PR discussion retrieval returned zero comments at this review point.

## Local build-reference provenance — 2026-11-24

The owner supplied read-only FileVersionInfo and SHA-256 output for all four DLLs required by the documented direct Roslyn build. No local absolute paths or user identity are recorded.

| File | File version | Product version | SHA-256 |
| --- | --- | --- | --- |
| Dalamud.dll | 15.0.4.1 | 15.0.4.1+d0e93484f99fdb7d58b3a2f22152cf108f3486ff | 57bd0e4e7f1fcb00325b90f369665b37feb73084307a1374734f33fdfe52b257 |
| Dalamud.Common.dll | 1.0.0.0 | 1.0.0+d0e93484f99fdb7d58b3a2f22152cf108f3486ff | 294a1f513df5faf5847fb72df63100ced46b1b3307daf9490c9742fab476eabb |
| Dalamud.Bindings.ImGui.dll | 1.0.0.0 | 1.0.0+d0e93484f99fdb7d58b3a2f22152cf108f3486ff | a8487bf7cd26f86348ad5c2fe5eebd5b8704e989013601a91388e2ca81207c9b |
| Lumina.dll | 7.7.0.0 | 7.7.0+b92c0cbdb65c9e53b0ec5faafa83b2cc32aa9a7c | 6b92e0ba9f8370358de57887812744548054e40640af29a3d101f80d8cd8f614 |

- Verified [Dalamud commit d0e9348](https://github.com/goatcorp/Dalamud/tree/d0e93484f99fdb7d58b3a2f22152cf108f3486ff), dated 2026-11-18, commit message "build: 15.0.4.1". Its LICENSE blob (0ad25db4bd1d86c452db3f9602ccdbe172438f52) matches the staged AGPLv3 text.
- Verified [Lumina commit b92c0cb](https://github.com/NotAdam/Lumina/tree/b92c0cbdb65c9e53b0ec5faafa83b2cc32aa9a7c), dated 2026-08-25. Its LICENSE blob (5c93f45654687732c6b9b568186deb766f78a81c) contains WTFPL version 2. The pinned Dalamud Directory.Packages.props explicitly references Lumina 7.7.0.
- These are owner-reported local reference fingerprints and verified embedded-revision mappings, not independent hashes of upstream release binaries, proof of a hermetic rebuild, or proof that every runtime dependency file matches an upstream artifact. None of these four DLLs is bundled by SimBridge. The source revision identification gate is satisfied; binary provenance verification where necessary and the scope of corresponding-source obligations are separate questions.

## Targeted expression and corresponding-source review — 2026-11-24

Reviewed the actual staging source at 6def6c6, including the module lifecycle adapter, role synchronization, native initialization and compiler/package inputs. A targeted lexical comparison of six SimBridge adapters against the corresponding AnoMech AI and BossMod encounter files at the pinned release revisions found a concrete matching Apocalypse group-swap sequence and a matching Ultimate Relativity laser-order switch. Both are also present at the previously consulted development revisions. The Apocalypse match includes the adjuster selection and swap expression, not merely a type name. Other comparisons did not yield similarly substantial exact sequences. This limited token-based check does not prove absence of nonliteral adaptation, independent authorship or exhaustive provenance.

- Record the Apocalypse and Ultimate Relativity adapted logic explicitly in THIRD_PARTY_NOTICES.md, with pinned file links and applicable upstream license terms. Preserve the complete upstream notices already staged. Credit other reviewed strategy/component references without asserting copied expression.
- The code directly references Dalamud API assemblies, constructs and activates native BossMod encounter modules, invokes native calculations, reads and writes owned component state, and subscribes to upstream events. Do not rely on separate installation or reflection alone to classify this as an independent work.
- Conservative proposed release approach: publish SimBridge's own source under its approved AGPL-3.0-or-later grant, preserve upstream AGPLv3 and permissive license terms, and provide exact upstream source and build-access directions for the closely integrated dependencies as potentially required corresponding source. Do not relicense those upstream projects. This is a conservative release approach, not a definitive legal conclusion about combined-work scope.
- AGPLv3 sections 1 and 6(d) permit third-party source hosting with equivalent free access and clear directions alongside the binary; the distributor remains responsible for availability. Section 13 also addresses modified programs with remote network interaction; this local bridge does not itself implement a remote-user service. Do not mistake the local repository test for such a service. See the [license text](https://www.gnu.org/licenses/agpl-3.0.html) and [FSF FAQ](https://www.gnu.org/licenses/gpl-faq.html#GPLPlugins).
- The exact Dalamud tree has ten Git submodules, so a GitHub snapshot ZIP alone would omit their contents. Record recursive checkout instructions in SOURCE.md, using the pinned commit and its gitlinks. The pinned AnoMech and BossMod trees have no Git submodules at those revisions. Transitive package sources, nested submodule availability, any additional applicable notices, and completeness of the source delivery path must still be checked; do not claim all corresponding-source obligations are satisfied.

## Recursive source access and build dependency inventory — 2026-11-24

All ten direct and five nested gitlinks in the pinned Dalamud 15.0.4.1 tree resolved to complete GitHub tree metadata via authenticated repository reads. The nested gitlinks are two EXDSchema snapshots under Lumina.Excel and the underlying gc-imgui, gc-ImGuizmo and gc-implot repositories. The original Nomade040/nmd URL returned a redirect; the renamed gh-nomad/nmd repository resolves the exact pinned commit. No further nested gitlinks were found in those five trees. This verifies source-tree reachability via the connector, not an anonymous recursive clone/build or complete archive-byte retrieval.

Direct package declarations reviewed (not a complete restored transitive dependency graph):

- AnoMech/AnoMech.csproj: Dalamud.NET.Sdk 15.0.0, NAudio.Core 2.2.1, NAudio.WinMM 2.2.1, NVorbis 0.10.5, build analyzer Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0; relay project source is in the same tree.
- BossMod/BossModReborn.csproj: DalamudPackager 15.0.0, RebornMaterial 7.5.8.4, TerraFX.Interop.Windows 10.0.28000, local Dalamud references and in-tree source generator. Source generator references Microsoft.CodeAnalysis.CSharp 5.9.0. Upstream third-party notices cover RebornMaterial and KamiToolKit; exact package source mapping remains to be reviewed.
- Dalamud/Directory.Packages.props records central versions and includes Lumina 7.7.0, HexaGen.Runtime 1.1.20, goatcorp.Reloaded.Hooks 4.2.2, Reloaded.Memory 7.0.0, Reloaded.Memory.Buffers 2.0.0 and other libraries. Consult that exact file and each project/lock file for the complete declared list.
- Lumina/src/Lumina/Lumina.csproj: BCnEncoder.Net 2.2.1, Microsoft.Extensions.ObjectPool 10.0.0, build-only MinVer 6.0.0.

SimBridge's tested direct compiler script references only the four local Dalamud/Lumina assemblies and the .NET reference pack. It ships none of these upstream packages. The wider inventory is conservative given the close runtime integration. No blanket System Libraries exclusion or complete package-source/compatibility certification is asserted.

## Additional dependency mapping and preservation decision — 2026-11-24

The owner approved pinned upstream source references under AGPLv3 section 6(d), combined with a recoverable-source preservation policy. The operational policy is in SOURCE.md and travels with the binary. It does not waive any corresponding-source obligation.

- RebornMaterial tag 7.5.8.4 resolves to 0c74204ecfd6785c27ac53213950c3ad08337e63; the LICENSE is LGPLv3 and its project declares LGPL-3.0-or-later. BossMod's pinned packages.lock.json resolves RebornMaterial 7.5.8.4, DalamudPackager 15.0.0 and TerraFX.Interop.Windows 10.0.28000, with no other package entries in that lock. RebornMaterial's pinned project and lock use Dalamud.NET.Sdk 15.0.0, framework assembly references (including FFXIVClientStructs), and DotNet.ReproducibleBuilds 2.0.5. No Git submodules in its reviewed tree. Binary-to-source cryptographic equivalence not verified.
- NAudio v2.2.1 resolves to b5d5ff83fd378f046398891fe5cd99426ce44732; its license.txt is MIT (Copyright 2020 Mark Heath). NVorbis v0.10.5 resolves to 519d4e2aae7d6a4d5bab552ec5c1e517e9c78855; its LICENSE is MIT (Copyright 2020 Andrew Ward). Both exact trees were reachable and have no Git submodules. These are source-tag mappings for the versions declared by AnoMech, not local binary verification.
- Public NuGet pages for NAudio.Core/WinMM 2.2.1, NVorbis 0.10.5, Dalamud.NET.Sdk 15.0.0, DalamudPackager 15.0.0 and TerraFX.Interop.Windows 10.0.28000 were retrieved. SDK and Packager are build helpers, not used by SimBridge's tested direct Roslyn path. RebornMaterial's NuGet page could not be retrieved by the public browsing tool; the pinned upstream source and BossMod lock were retrieved through the repository connector. No exact source mapping for every build-tool/package artifact is asserted.
- Public browsing retrieved the exact Lumina source page; attempts to retrieve the exact AnoMech, BossMod, Dalamud and RebornMaterial commit pages returned cache misses. Authenticated repository reads had succeeded. This is inconclusive for anonymous retrieval, not proof those sources are unavailable.
- Source preservation and recovery instructions are approved, but no upstream backup archives or recurring availability monitoring have been created in this review. An actual anonymous source retrieval and backup restoration check, and review of remaining required transitive package sources and exclusions, are outstanding. Do not represent the adoption of the policy as completed compliance.

## Open pre-publication gates

- Owner-side authorization and targeted adapted-logic attribution are recorded. Review any additional adapted expressions or applicable copyright notices found during final review; the limited comparison does not certify the entire code provenance.
- All four principal upstream source revisions and license texts are identified above. Where required for the compliance review, match local binaries to upstream published assets; do not claim this match from version metadata alone.
- Assess combined-work and corresponding-source obligations for the intended binary distribution and preserve applicable upstream terms. Provide complete corresponding source, build/install scripts and any additional required source access for the exact release, rather than assuming a repository link alone satisfies every obligation.
- Verify license and notice files accompany distributed assets where required. The public release is blocked pending resolution.

## Privacy audit (initial)

Reachable main and staging branch history before this documentation commit: 14 commits, 55 complete file-change patches checked using patterns for emails, personal absolute paths, obvious keys/tokens and sensitive filenames. No matches found. All author identities used the azazev GitHub noreply address; the initial GitHub automated committer used noreply@github.com. No upstream binary or diagnostic log was found in the reviewed patches. The original verified DLL's printable ASCII and UTF-16LE strings were screened for these patterns; no matches or PDB filenames found.

This is a bounded static screening, not an exhaustive security or forensic audit. Tag listing was unavailable through the connector. Recheck final refs, PR discussions, final release archive, metadata and history after remaining edits; unreachable Git objects and external records were not audited.

## Preservation and supplemental provenance checkpoint — 2026-11-25

The release-specific supplemental source index and scope assessment in [SOURCE.md](SOURCE.md) supersede the preliminary package mapping and preservation statements above. Six earlier candidate revisions were corrected using publisher metadata; 28/28 nuspec records retrieved, 19 with commit metadata and nine without. The owner preserved 22 original roots/gitlinks and 24 supplemental snapshots, including two scoped source archives and both HexaGen revisions. All 46 primary and secondary checksum checks are evidenced by owner reports; archive bytes were not independently rehashed. No upstream package binaries are added to the installer. No repeated inventory or additional source download is planned for this working scope. Unresolved native/HexaGen/tag candidate artifact equivalence, any required corresponding-source obligations beyond this scope and final source-delivery access remain explicit qualifications; no legal certification. The packaging and privacy gates remain open.
