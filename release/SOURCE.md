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

These tags resolve to the listed commits. The package binaries have not been independently matched to these trees. Preserve each upstream license. For general-purpose build tools, see the versioned package descriptions at https://www.nuget.org/packages/Dalamud.NET.Sdk/15.0.0 and https://www.nuget.org/packages/DalamudPackager/15.0.0. The tested SimBridge direct compiler does not use either package; this does not exclude any necessary upstream build inputs. The exact TerraFX.Interop.Windows 10.0.28000 package record is https://www.nuget.org/packages/TerraFX.Interop.Windows/10.0.28000; its publisher-declared source revision is listed below. Other package declarations are in the pinned upstream project and lock files. No claim is made that package metadata alone supplies corresponding source.


## Supplemental dependency source index

These references cover the reviewed installed Dalamud package inventory and additional BossMod TerraFX version; Lumina is listed above. They are supplemental provenance and source-access evidence for separately installed dependencies, not a list of binaries shipped by SimBridge. Links identify full upstream trees; local scoped archives are not standalone complete build trees. Preserve original upstream license and notice terms. Obtain a checkout with `git clone https://github.com/OWNER/REPO.git` followed by `git checkout COMMIT` in that repository, using the exact repository and revision below. Where the original nuspec repository URL differs, the linked repository was verified to expose the specified revision.

| Package/version | Pinned source | License/evidence qualification |
| --- | --- | --- |
| goatcorp.Reloaded.Hooks 4.2.2; Hooks.Definitions 1.14.0 candidate | [goatcorp/goatcorp.Reloaded.Hooks @ ec11de71418e](https://github.com/goatcorp/goatcorp.Reloaded.Hooks/tree/ec11de71418e025cd28513a377c0fa44278977ee) | LGPL-3.0; Definitions mapping candidate |
| goatcorp.Reloaded.Memory.Buffers 2.1.0 | [goatcorp/goatcorp.Reloaded.Memory.Buffers @ 1460aa95c9e8](https://github.com/goatcorp/goatcorp.Reloaded.Memory.Buffers/tree/1460aa95c9e8a298e7b21ffa540f24447e318ebf) | LGPL-3.0 |
| HexaGen.Runtime 1.1.20 (publisher revision) | [HexaEngine/HexaGen @ 5023fcf76da3](https://github.com/HexaEngine/HexaGen/tree/5023fcf76da384ddd61129c64f4372d3a604e84b) | MIT |
| HexaGen.Runtime 1.1.20 (version-correcting successor) | [HexaEngine/HexaGen @ d2c34f7cdc10](https://github.com/HexaEngine/HexaGen/tree/d2c34f7cdc1041e8bd335dced257a219e0d34846) | MIT; candidate |
| TerraFX.Interop.Windows 10.0.26100.5 | [terrafx/terrafx.interop.windows @ 24d0b5837c6a](https://github.com/terrafx/terrafx.interop.windows/tree/24d0b5837c6a33377fa0c4cdf165a7aa174fd21c) | MIT |
| TerraFX.Interop.Windows 10.0.28000 | [terrafx/terrafx.interop.windows @ 958aaa9cf1a5](https://github.com/terrafx/terrafx.interop.windows/tree/958aaa9cf1a53318cc33eeb33f5d2e2deb364de1) | MIT |
| Newtonsoft.Json 13.0.4 | [JamesNK/Newtonsoft.Json @ 4e13299d4b0e](https://github.com/JamesNK/Newtonsoft.Json/tree/4e13299d4b0ec96bd4df9954ef646bd2d1b5bf2a) | MIT |
| Reloaded.Memory 7.0.0 | [Reloaded-Project/Reloaded.Memory @ c3a7da895d44](https://github.com/Reloaded-Project/Reloaded.Memory/tree/c3a7da895d44dc188ae7a488341b8a0db10db175) | LGPL-3.0; tag candidate |
| BCnEncoder.Net 2.2.1 | [Nominom/BCnEncoder.NET @ 7173400b43f1](https://github.com/Nominom/BCnEncoder.NET/tree/7173400b43f18e646ba6dc924a070296638b2d9e) | MIT OR Unlicense |
| BitFaster.Caching 2.5.4 | [bitfaster/BitFaster.Caching @ 28e46925eff3](https://github.com/bitfaster/BitFaster.Caching/tree/28e46925eff3b402e7466c4e081d03b36d12b228) | MIT |
| CheapLoc 1.1.8 | [goaaats/CheapLoc @ 58b6c4550a4e](https://github.com/goaaats/CheapLoc/tree/58b6c4550a4ef18a3a21efa9ff526dcf48398c2a) | MIT; version candidate |
| CommunityToolkit.HighPerformance 8.4.0 | [CommunityToolkit/dotnet @ 638b41dad30d](https://github.com/CommunityToolkit/dotnet/tree/638b41dad30dffabb123a39aa38eabc7e3721371) | MIT |
| Iced 1.17.0 | [icedland/iced @ c7aa5116314e](https://github.com/icedland/iced/tree/c7aa5116314e90d391a3350878f9fb99192aafb6) | MIT |
| JetBrains.Annotations 2025.2.4 | [JetBrains/JetBrains.Annotations @ 332ebae986c8](https://github.com/JetBrains/JetBrains.Annotations/tree/332ebae986c8887bdbcd5525bdadbf962b1b56a3) | MIT; tag candidate |
| Mono.Cecil 0.11.3 | [jbevain/cecil @ e9a4815fdb10](https://github.com/jbevain/cecil/tree/e9a4815fdb102a3bc87a606c6e9ee5f61c00414a) | MIT; tag candidate |
| Serilog 4.3.0 | [serilog/serilog @ 726e29c5b172](https://github.com/serilog/serilog/tree/726e29c5b172aa8813285be1ad8cc728fd531eab) | Apache-2.0 |
| Serilog.Sinks.Async 2.1.0 | [serilog/serilog-sinks-async @ 5c6e5df7dffe](https://github.com/serilog/serilog-sinks-async/tree/5c6e5df7dffe5535978475c9ce8c5c6fb1cc9dca) | Apache-2.0 |
| Serilog.Sinks.Console 6.1.1 | [serilog/serilog-sinks-console @ b3cb1f042f56](https://github.com/serilog/serilog-sinks-console/tree/b3cb1f042f5636161b831fa8816d70d865a5eec4) | Apache-2.0 |
| Serilog.Sinks.File 7.0.0 | [serilog/serilog-sinks-file @ 23c732a8658a](https://github.com/serilog/serilog-sinks-file/tree/23c732a8658a0df2a5434fe69b0011800b14f0da) | Apache-2.0 |
| sqlite-net-pcl 1.9.172 | [praeclarum/sqlite-net @ 5f72241035dd](https://github.com/praeclarum/sqlite-net/tree/5f72241035dd48f4305a7c811eba8e7c955e9840) | MIT |
| System.Reactive 6.1.0 | [dotnet/reactive @ f4da16f15a3c](https://github.com/dotnet/reactive/tree/f4da16f15a3cde97f178396ea6e3489cc893651f) | MIT |
| SQLitePCLRaw.bundle_green/core/lib.e_sqlite3/provider.e_sqlite3 2.1.2 | [ericsink/SQLitePCL.raw @ 1380c0b2dd48](https://github.com/ericsink/SQLitePCL.raw/tree/1380c0b2dd4868e615de9d4bae31a97163edc861) | Apache-2.0; tag candidate |
| Microsoft.Extensions.ObjectPool and System.Reflection.MetadataLoadContext 10.0.0 | [dotnet/dotnet @ b0f34d51fccc](https://github.com/dotnet/dotnet/tree/b0f34d51fccc69fd334253924abd8d6853fad7aa) | MIT; scoped local evidence |
| SQLite 3.39.2 native build source candidate | [ericsink/cb @ 9602a5f87dc6](https://github.com/ericsink/cb/tree/9602a5f87dc6c352e1b0638277814487334213de) | SQLite public domain; other files retain their own terms; scoped evidence |

Of 28 reviewed package/version records, 19 supplied publisher commit metadata, including Lumina and both Microsoft packages; nine did not. A tag or version candidate is not a confirmed published artifact mapping. HexaGen's publisher commit declares Runtime 1.1.19-b1; its immediate successor declares 1.1.20. Both are retained. The unified .NET source manifest identifies aspnetcore 7387de91234d3ef751fa50b3d1bfede4130213ff and runtime 60629d14374c56f1cb51819049ad1fa529307f8d; the full unified tree is the publisher reference. Local preservation selected ObjectPool and MetadataLoadContext directories and root notices/build metadata, not the full .NET toolchain. Native SQLite 3.39.2 is confirmed by the SQLitePCLRaw release notes; the cb revision remains a candidate, not a proven native artifact build. The scoped cb copy retains SQLite sources and relevant generator/scripts, not unrelated SQLCipher or precompiled binaries. The deprecated SQLitePCLRaw native dependency has a reported vulnerability; SimBridge does not bundle or update it, and exploit applicability to this release has not been established.

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

The owner has completed the defined primary and secondary preservation operations described below. Recurring monitoring remains a maintainer responsibility and has not been configured.

## Availability and release scope

The conservative release plan treats the closely integrated dependencies as potentially within corresponding-source scope. Preserve their own license terms and notices. AGPLv3 section 6(d) allows equivalent no-charge access from other servers when clear directions accompany the object code; the distributor remains responsible for maintaining required access. Verify anonymous access to the exact SimBridge source asset, the pinned upstream sources, required submodules and any additional corresponding source before publication. If a required source becomes unavailable, provide a lawful equivalent copy or suspend the affected binary distribution until access is restored.

These links and commands do not constitute a legal or exhaustive transitive dependency audit. Verify the final source-delivery path and resolve any required corresponding-source uncertainty before binary publication. No upstream DLLs are included in the proposed SimBridge installer ZIP.

See PROVENANCE.md in the source tree for the review and limitations.

## Release-specific scope assessment and limits (2026-11-25)

The distributed executable is SimBridge, with no bundled upstream DLLs. Its direct compiler uses standard .NET SDK/reference assemblies and four framework references. It specifically integrates with AnoMech and BossMod through intimate reflection and runtime control flow. For a conservative release approach, retain SimBridge's complete source, build/install tools, contract metadata, notices and pinned source access for the seven reviewed upstream roots and their fifteen nested/direct gitlinks. Do not rely on separate installation or reflection as a blanket independent-work exemption. The additional 24 snapshots are supplemental dependency evidence, not a conclusion that every indirect package is part of the covered work. Under AGPLv3 section 1, unmodified general-purpose compiler/SDK, packager, build analyzers and other standard build utilities are excluded from the SimBridge-specific source inventory on present evidence, not necessarily from an upstream project's own build documentation. Separate Injector-only dependencies, an unrelated installed BossMod plugin, unrelated SQLCipher and the unrelated portions of the general .NET toolchain are outside this working preservation scope because the reviewed SimBridge payload and integration do not specifically require those components as such. This is an engineering assessment, not a binding determination of combined-work status; if additional corresponding source is required, it must be accessible before binary distribution.

Owner reports establish successful credential-isolated retrieval and local verification of 22 original and 24 supplemental snapshots, plus 46/46 destination checksum passes for a separate backup location. The two scoped archives verify 248 and 14 selected Git blobs respectively. Actual archives and remote backup restoration were not independently inspected. These results complete the planned preservation operation; they do not prove installed binary equivalence, complete transitive corresponding source, or legal clearance. Maintain the source-access policy above; recheck necessary source availability before publication and at least quarterly thereafter. Unresolved exact artifact mappings remain documented rather than silently treated as verified. Do not publish binaries if required corresponding-source access cannot be established.
