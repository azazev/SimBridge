# Source access — private distribution draft

SimBridge v0.8.0-beta.1, developer azazev. SimBridge's own copyrightable contributions are licensed AGPL-3.0-or-later; see LICENSE and THIRD_PARTY_NOTICES.md.

The independently verified Windows build was made from SimBridge commit 0b011fb8bd1333154637ae0a422d2e80013bfb48. Subsequent changes are documentation/distribution preparation only; the plugin payload hashes remain the same.

## SimBridge

Planned release: https://github.com/azazev/SimBridge/releases/tag/v0.8.0-beta.1

Planned source asset: https://github.com/azazev/SimBridge/releases/download/v0.8.0-beta.1/SimBridge-v0.8.0-beta.1-source.zip

These URLs are planned, not currently published or accessible. The private packaging script prepares the source archive with build/install instructions, tests, license texts, and the reviewed source tree. Before public delivery, verify the tag points to the approved source, the source asset downloads without authentication, and all required corresponding source is available at no additional charge.

## Upstream release source references

- AnoMech 0.5.1.0: https://github.com/anomek/AnoMech/tree/6aec4a5940df56ebc4f70a2fdddeceb31c7848b8
- BossMod Reborn 7.5.8.10: https://github.com/FFXIV-CombatReborn/BossmodReborn/tree/05bdf3abd1963c84105784273a8c171bb7cf0b7c
- Dalamud 15.0.4.1 (API 15), including Common and Bindings.ImGui: https://github.com/goatcorp/Dalamud/tree/d0e93484f99fdb7d58b3a2f22152cf108f3486ff
- Lumina 7.7.0: https://github.com/NotAdam/Lumina/tree/b92c0cbdb65c9e53b0ec5faafa83b2cc32aa9a7c

The Dalamud and Lumina revisions are verified upstream commit IDs reported in the local development-reference assembly product versions. The local DLL fingerprints are recorded in PROVENANCE.md; official distributed binary equivalence is not independently established.

The upstream links are a reference index, not a conclusion that the source obligations for the combined work have been satisfied. Resolve applicable corresponding-source requirements, third-party notices and any necessary additional dependency source/build evidence before public distribution. No upstream DLLs are included in the proposed SimBridge installer ZIP.

See PROVENANCE.md in the source tree for the review and limitations.
