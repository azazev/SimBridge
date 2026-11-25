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

## Additional upstream dependency source references

- RebornMaterial 7.5.8.4 (BossMod declared/locked dependency): https://github.com/LTS-FFXIV/RebornMaterial/tree/0c74204ecfd6785c27ac53213950c3ad08337e63
- NAudio 2.2.1 (Core and WinMM; AnoMech declared dependencies): https://github.com/naudio/NAudio/tree/b5d5ff83fd378f046398891fe5cd99426ce44732
- NVorbis 0.10.5 (AnoMech declared dependency): https://github.com/NVorbis/NVorbis/tree/519d4e2aae7d6a4d5bab552ec5c1e517e9c78855

These tags resolve to the listed commits. The package binaries have not been independently matched to these trees. Preserve each upstream license. For general-purpose build tools, see the versioned package descriptions at https://www.nuget.org/packages/Dalamud.NET.Sdk/15.0.0 and https://www.nuget.org/packages/DalamudPackager/15.0.0. The tested SimBridge direct compiler does not use either package; this does not exclude any necessary upstream build inputs. The exact TerraFX.Interop.Windows 10.0.28000 package record is https://www.nuget.org/packages/TerraFX.Interop.Windows/10.0.28000; exact source revision mapping has not been established. Other package declarations are in the pinned upstream project and lock files. No claim is made that package metadata alone supplies corresponding source.

## Obtaining pinned upstream source

The source links above identify the reviewed source snapshots. For a checkout including recorded submodules, use Git rather than relying solely on generated snapshot archives:

    git clone https://github.com/anomek/AnoMech.git
    git -C AnoMech checkout 6aec4a5940df56ebc4f70a2fdddeceb31c7848b8
    git clone https://github.com/FFXIV-CombatReborn/BossmodReborn.git
    git -C BossmodReborn checkout 05bdf3abd1963c84105784273a8c171bb7cf0b7c
    git clone https://github.com/goatcorp/Dalamud.git
    git -C Dalamud checkout d0e93484f99fdb7d58b3a2f22152cf108f3486ff
    git -C Dalamud submodule update --init --recursive
    git clone https://github.com/NotAdam/Lumina.git
    git -C Lumina checkout b92c0cbdb65c9e53b0ec5faafa83b2cc32aa9a7c

Use the upstream build documentation and package declarations at those revisions. The pinned Dalamud tree has ten submodules whose revisions are recorded by Git; the checkout command obtains them recursively. Generated repository ZIPs alone omit submodule contents. The original Nomade040/nmd URL redirects to https://github.com/gh-nomad/nmd; the exact pinned commit is available there. All ten direct and five nested gitlink trees were reachable in the release review. Consult the upstream project/package declarations for their additional build dependencies; see PROVENANCE.md for scope and remaining checks.

For the additional pinned source trees:

    git clone https://github.com/LTS-FFXIV/RebornMaterial.git
    git -C RebornMaterial checkout 0c74204ecfd6785c27ac53213950c3ad08337e63
    git clone https://github.com/naudio/NAudio.git
    git -C NAudio checkout b5d5ff83fd378f046398891fe5cd99426ce44732
    git clone https://github.com/NVorbis/NVorbis.git
    git -C NVorbis checkout 519d4e2aae7d6a4d5bab552ec5c1e517e9c78855

## Source preservation and availability policy

Adopted for SimBridge releases with owner approval, 2026-11-24. The primary source delivery method is AGPLv3 section 6(d): distribute the complete required source for SimBridge alongside its binary, with pinned, equivalent no-charge access to any additional required upstream source. This policy does not itself determine the legal scope of the covered work.

Before each binary release:
1. Freeze the tested source revision, build/install scripts, dependency manifest/lock information, notices, binary and source hashes. Identify and document required upstream and transitive source, including any required generated inputs, patches and recursive submodules. Record specific grounds for any exclusions under section 1 rather than excluding separately installed dependencies as a class.
2. Verify anonymous no-charge retrieval of all required source, not just the repository landing page; for recursive sources verify checkout of every required gitlink. Where third-party package metadata cannot identify the exact corresponding source, resolve the mapping or keep distribution on hold.
3. Preserve a recoverable copy of the exact required sources and build inputs, including upstream licenses and notices, in a separate access-controlled backup; use ordinary publicly documented formats. Keep a second copy or other recoverable storage independent of the primary hosting location. Record checksums and test restoration. Do not include game binaries, user logs, credentials or other material without redistribution rights. Do not treat this policy as evidence that backups already exist.
4. Publish the exact SimBridge source archive next to the binary at no extra charge; provide clear upstream source directions in SOURCE.md and on the release page. Do not gate source behind donations, account access, or private repositories. Publish additional lawful source mirrors if necessary.
5. Keep source access for as long as required by the applicable license. Check source links and retained copies at every release and at least quarterly while affected binaries are offered. If required source becomes inaccessible, restore lawful equivalent access promptly; suspend affected binary downloads and feed entries until access is restored. Preserve obligations to prior recipients; removing a feed entry alone does not discharge them.
6. Record source issues through the project's GitHub Issues channel after it is public, including release/version and affected link. Never request payment for required source access.

The backup procedure and recurring check are maintainer responsibilities, not an assertion that storage or monitoring has already been configured.

## Availability and release scope

The conservative release plan treats the closely integrated dependencies as potentially within corresponding-source scope. Preserve their own license terms and notices. AGPLv3 section 6(d) allows equivalent no-charge access from other servers when clear directions accompany the object code; the distributor remains responsible for maintaining required access. Verify anonymous access to the exact SimBridge source asset, the pinned upstream sources, required submodules and any additional corresponding source before publication. If a required source becomes unavailable, provide a lawful equivalent copy or suspend the affected binary distribution until access is restored.

These links and commands are not a completed legal or transitive dependency audit. Verify the necessary build/install scripts, additional package sources and notices before closing the compliance gate. No upstream DLLs are included in the proposed SimBridge installer ZIP.

See PROVENANCE.md in the source tree for the review and limitations.
