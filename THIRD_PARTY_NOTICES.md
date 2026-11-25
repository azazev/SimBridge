# Third-party notices

SimBridge is an independent compatibility project by azazev. It is not endorsed by or officially affiliated with the projects below. The SimBridge license applies to SimBridge's own contributions; it does not relicense upstream components.

## Upstream projects

- [AnoMech](https://github.com/anomek/AnoMech) — FFXIV mechanics simulator; credit to its maintainers and contributors. GNU Affero General Public License version 3: [upstream license](https://github.com/anomek/AnoMech/blob/6aec4a5940df56ebc4f70a2fdddeceb31c7848b8/LICENSE.md). The complete AGPLv3 text is included in [LICENSE](LICENSE).
- [BossMod Reborn](https://github.com/FFXIV-CombatReborn/BossmodReborn) — encounter modules and mechanic guidance; credit to Andrew Gilewsky and upstream maintainers and contributors. BSD 3-Clause License, copyright (c) 2022-2024, Andrew Gilewsky. Full unchanged text: [LICENSES/BossModReborn-BSD-3-Clause.txt](LICENSES/BossModReborn-BSD-3-Clause.txt); [upstream license](https://github.com/FFXIV-CombatReborn/BossmodReborn/blob/05bdf3abd1963c84105784273a8c171bb7cf0b7c/LICENSE).
- [Dalamud](https://github.com/goatcorp/Dalamud) — plugin framework/API; AGPL-3.0 license verified at the source revision reported by the local 15.0.4.1 build. See its [license](https://github.com/goatcorp/Dalamud/blob/d0e93484f99fdb7d58b3a2f22152cf108f3486ff/LICENSE). The full AGPLv3 text is included in LICENSE.
- [Dalamud.NET.Sdk](https://github.com/goatcorp/Dalamud.NET.Sdk) — optional standard project build SDK; upstream repository identifies MIT. See its [license](https://github.com/goatcorp/Dalamud.NET.Sdk/blob/master/LICENSE.txt).
- [Lumina](https://github.com/NotAdam/Lumina) — locally referenced framework dependency; WTFPL version 2, verified at the source revision reported by the local 7.7.0 build. See its [license](https://github.com/NotAdam/Lumina/blob/b92c0cbdb65c9e53b0ec5faafa83b2cc32aa9a7c/LICENSE). No Lumina binary is bundled.

BossMod Reborn's [third-party notices at release 7.5.8.10](https://github.com/FFXIV-CombatReborn/BossmodReborn/blob/05bdf3abd1963c84105784273a8c171bb7cf0b7c/THIRD_PARTY_NOTICES.md) identify KamiToolKit (MIT) and RebornMaterial (LGPL-3.0-or-later). Those assemblies and overlay assets are not included in the verified SimBridge plugin payload. Any corresponding-source obligations extending to upstream components remain under review.

## Specific adapted logic and integration references

- **Apocalypse — AnoMech:** `source/RoleSync.cs`, `ResolveGroups`, adapts the Dark Water group-adjustment algorithm (support/DPS adjuster selection and swap) from [FruP3ApocalypseAi.cs, AdjustWaterGroups/Adjusters](https://github.com/anomek/AnoMech/blob/6aec4a5940df56ebc4f70a2fdddeceb31c7848b8/AnoMech/Scenarios/Fru/P3Apocalypse/FruP3ApocalypseAi.cs). Credit to AnoMech's copyright holders and contributors; the applicable upstream AGPLv3 terms are retained. This attribution does not claim ownership of the upstream expression.
- **Ultimate Relativity — BossMod Reborn:** `source/RelativitySync.cs` adapts the native laser-order mapping in [P3UltimateRelativity.cs](https://github.com/FFXIV-CombatReborn/BossmodReborn/blob/05bdf3abd1963c84105784273a8c171bb7cf0b7c/BossMod/Modules/Dawntrail/Ultimate/FRU/P3UltimateRelativity.cs). The BSD-3-Clause copyright and full license are retained as noted above.
- **Other integration references:** the Darklit Dragonsong, Ultimate Relativity and Crystallize Time role-mapping adapters consult the corresponding AnoMech strategies and BossMod components; Diamond Dust and Light Rampant invoke native BossMod calculations. This is attribution for the reviewed integration, not a claim that all expression was copied or that no further attribution is necessary.

## Distribution scope

The verified baseline contains only SimBridge's DLL, manifest and dependency metadata in the plugin directory; upstream DLLs and assets are not bundled. Reflection contracts contain upstream identifiers, member shapes and method fingerprints; they do not package the upstream assemblies. The original source snapshots and resolved published release tags are documented in [release/PROVENANCE.md](release/PROVENANCE.md). See [release/SOURCE.md](release/SOURCE.md) for the planned corresponding-source index. These are not claimed as cryptographic matches for every installed dependency binary.

The license texts and attributions above are included conservatively. A dependency-only arrangement is not a determination of independent-work status under AGPL. Before publication, resolve any additional copyright notices, applicable licenses and corresponding-source requirements for the precise distributed work and dependency versions. Do not imply upstream endorsement.

See [release/PROVENANCE.md](release/PROVENANCE.md) for the remaining review.
