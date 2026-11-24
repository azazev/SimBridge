# Third-party notices

SimBridge is an independent compatibility project by azazev. It is not endorsed by or officially affiliated with the projects below. The SimBridge license applies to SimBridge's own contributions; it does not relicense upstream components.

## Upstream projects

- [AnoMech](https://github.com/anomek/AnoMech) — FFXIV mechanics simulator; credit to its maintainers and contributors. GNU Affero General Public License version 3: [upstream license](https://github.com/anomek/AnoMech/blob/1646a7068c2071a3d1fd9e3c84550dfd3f1b6ade/LICENSE.md). The complete AGPLv3 text is included in [LICENSE](LICENSE).
- [BossMod Reborn](https://github.com/FFXIV-CombatReborn/BossmodReborn) — encounter modules and mechanic guidance; credit to Andrew Gilewsky and upstream maintainers and contributors. BSD 3-Clause License, copyright (c) 2022-2024, Andrew Gilewsky. Full unchanged text: [LICENSES/BossModReborn-BSD-3-Clause.txt](LICENSES/BossModReborn-BSD-3-Clause.txt); [upstream license](https://github.com/FFXIV-CombatReborn/BossmodReborn/blob/35a908ad71b1a94fdbf342518ed78c7c72c458c0/LICENSE).
- [Dalamud](https://github.com/goatcorp/Dalamud) — plugin framework/API; upstream repository identifies AGPL-3.0. See its [license](https://github.com/goatcorp/Dalamud/blob/master/LICENSE).
- [Dalamud.NET.Sdk](https://github.com/goatcorp/Dalamud.NET.Sdk) — optional standard project build SDK; upstream repository identifies MIT. See its [license](https://github.com/goatcorp/Dalamud.NET.Sdk/blob/master/LICENSE.txt).
- [Lumina](https://github.com/NotAdam/Lumina) — locally referenced framework dependency; upstream repository identifies WTFPL. See its upstream repository for applicable license and notices.

BossMod Reborn's [third-party notices at release 7.5.8.10](https://github.com/FFXIV-CombatReborn/BossmodReborn/blob/05bdf3abd1963c84105784273a8c171bb7cf0b7c/THIRD_PARTY_NOTICES.md) identify KamiToolKit (MIT) and RebornMaterial (LGPL-3.0-or-later). Those assemblies and overlay assets are not included in the verified SimBridge plugin payload. Any corresponding-source obligations extending to upstream components remain under review.

## Distribution scope

The verified baseline contains only SimBridge's DLL, manifest and dependency metadata in the plugin directory; upstream DLLs and assets are not bundled. Reflection contracts contain upstream identifiers, member shapes and method fingerprints; they do not package the upstream assemblies. The original source snapshots and resolved published release tags are documented in [release/PROVENANCE.md](release/PROVENANCE.md). See [release/SOURCE.md](release/SOURCE.md) for the planned corresponding-source index. These are not claimed as cryptographic matches for every installed dependency binary.

The license texts and attributions above are included conservatively. A dependency-only arrangement is not a determination of independent-work status under AGPL. Before publication, resolve any additional copyright notices, applicable licenses and corresponding-source requirements for the precise distributed work and dependency versions. Do not imply upstream endorsement.

See [release/PROVENANCE.md](release/PROVENANCE.md) for the remaining review.
