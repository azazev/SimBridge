# Third-party notices

SimBridge is an independent compatibility project by azazev. It is not endorsed by or officially affiliated with the projects below. The SimBridge license applies to SimBridge's own contributions; it does not relicense upstream components.

## Upstream projects

- [AnoMech](https://github.com/anomek/AnoMech) — FFXIV mechanics simulator; credit to its maintainers and contributors. GNU Affero General Public License version 3: [upstream license](https://github.com/anomek/AnoMech/blob/6aec4a5940df56ebc4f70a2fdddeceb31c7848b8/LICENSE.md). The complete AGPLv3 text is included in [LICENSE](LICENSE).
- [BossMod Reborn](https://github.com/FFXIV-CombatReborn/BossmodReborn) — encounter modules and mechanic guidance; credit to Andrew Gilewsky and upstream maintainers and contributors. BSD 3-Clause License, copyright (c) 2022-2024, Andrew Gilewsky. Full unchanged text: [LICENSES/BossModReborn-BSD-3-Clause.txt](LICENSES/BossModReborn-BSD-3-Clause.txt); [upstream license](https://github.com/FFXIV-CombatReborn/BossmodReborn/blob/05bdf3abd1963c84105784273a8c171bb7cf0b7c/LICENSE).
- [Dalamud](https://github.com/goatcorp/Dalamud) — plugin framework/API; AGPL-3.0 license verified at the source revision reported by the local 15.0.4.1 build. See its [license](https://github.com/goatcorp/Dalamud/blob/d0e93484f99fdb7d58b3a2f22152cf108f3486ff/LICENSE). The full AGPLv3 text is included in LICENSE.
- [Dalamud.NET.Sdk](https://github.com/goatcorp/Dalamud.NET.Sdk) — optional standard project build SDK; upstream repository identifies MIT. See its [license](https://github.com/goatcorp/Dalamud.NET.Sdk/blob/master/LICENSE.txt).
- [Lumina](https://github.com/NotAdam/Lumina) — locally referenced framework dependency; WTFPL version 2, verified at the source revision reported by the local 7.7.0 build. See its [license](https://github.com/NotAdam/Lumina/blob/b92c0cbdb65c9e53b0ec5faafa83b2cc32aa9a7c/LICENSE). No Lumina binary is bundled.

BossMod Reborn's [third-party notices at release 7.5.8.10](https://github.com/FFXIV-CombatReborn/BossmodReborn/blob/05bdf3abd1963c84105784273a8c171bb7cf0b7c/THIRD_PARTY_NOTICES.md) identify KamiToolKit (MIT) and RebornMaterial (LGPL-3.0-or-later). Those assemblies and overlay assets are not included in the verified SimBridge plugin payload. See the release-specific source scope assessment and qualifications in [release/SOURCE.md](release/SOURCE.md).

## Additional pinned dependency references

- RebornMaterial 7.5.8.4: [source](https://github.com/LTS-FFXIV/RebornMaterial/tree/0c74204ecfd6785c27ac53213950c3ad08337e63), [LGPLv3 text](https://github.com/LTS-FFXIV/RebornMaterial/blob/0c74204ecfd6785c27ac53213950c3ad08337e63/LICENSE); upstream project declares LGPL-3.0-or-later, copyright The Combat Reborn Team per BossMod's notices.
- NAudio 2.2.1 (Core and WinMM): [source](https://github.com/naudio/NAudio/tree/b5d5ff83fd378f046398891fe5cd99426ce44732), [MIT license](https://github.com/naudio/NAudio/blob/b5d5ff83fd378f046398891fe5cd99426ce44732/license.txt); copyright 2020 Mark Heath.
- NVorbis 0.10.5: [source](https://github.com/NVorbis/NVorbis/tree/519d4e2aae7d6a4d5bab552ec5c1e517e9c78855), [MIT license](https://github.com/NVorbis/NVorbis/blob/519d4e2aae7d6a4d5bab552ec5c1e517e9c78855/LICENSE); copyright 2020 Andrew Ward.

These are upstream dependencies of the separately installed plugins, not binaries redistributed in SimBridge's installer. Preserve original license/notice files if additional source archives are distributed. See [release/SOURCE.md](release/SOURCE.md) for the approved source availability policy and release checks.

## Specific adapted logic and integration references

- **Apocalypse — AnoMech:** `source/RoleSync.cs`, `ResolveGroups`, adapts the Dark Water group-adjustment algorithm (support/DPS adjuster selection and swap) from [FruP3ApocalypseAi.cs, AdjustWaterGroups/Adjusters](https://github.com/anomek/AnoMech/blob/6aec4a5940df56ebc4f70a2fdddeceb31c7848b8/AnoMech/Scenarios/Fru/P3Apocalypse/FruP3ApocalypseAi.cs). Credit to AnoMech's copyright holders and contributors; the applicable upstream AGPLv3 terms are retained. This attribution does not claim ownership of the upstream expression.
- **Ultimate Relativity — BossMod Reborn:** `source/RelativitySync.cs` adapts the native laser-order mapping in [P3UltimateRelativity.cs](https://github.com/FFXIV-CombatReborn/BossmodReborn/blob/05bdf3abd1963c84105784273a8c171bb7cf0b7c/BossMod/Modules/Dawntrail/Ultimate/FRU/P3UltimateRelativity.cs). The BSD-3-Clause copyright and full license are retained as noted above.
- **Other integration references:** the Darklit Dragonsong, Ultimate Relativity and Crystallize Time role-mapping adapters consult the corresponding AnoMech strategies and BossMod components; Diamond Dust and Light Rampant invoke native BossMod calculations. This is attribution for the reviewed integration, not a claim that all expression was copied or that no further attribution is necessary.

## Distribution scope

The verified baseline contains only SimBridge's DLL, manifest and dependency metadata in the plugin directory; upstream DLLs and assets are not bundled. Reflection contracts contain upstream identifiers, member shapes and method fingerprints; they do not package the upstream assemblies. The original source snapshots and resolved published release tags are documented in [release/PROVENANCE.md](release/PROVENANCE.md). See [release/SOURCE.md](release/SOURCE.md) for the planned corresponding-source index. These are not claimed as cryptographic matches for every installed dependency binary.

The license texts and attributions above are included conservatively. A dependency-only arrangement is not a determination of independent-work status under AGPL. The supplemental dependency index in [release/SOURCE.md](release/SOURCE.md) provides pinned links and license identifiers for the reviewed indirect dependencies, including LGPL and Apache-2.0 packages. These packages are not shipped as binaries by SimBridge; the original upstream license/notice files remain with their source. The complete scope and source-access obligations must be satisfied before binary publication. Do not imply upstream endorsement.

See [release/PROVENANCE.md](release/PROVENANCE.md) for the remaining review.
