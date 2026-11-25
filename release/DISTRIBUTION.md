# Phase 4 — private distribution preparation

The first public release is proposed as v0.8.0-beta.1. AssemblyVersion remains 0.8.0.0; the legacy internal identity remains AnoBossBridge. Do not rename the three matching plugin files.

## Prepare the verified payload on Windows

Download the reviewed staging checkout. In its root, run:

    .\release\Prepare-Distribution.ps1 -PluginDirectory 'PATH-TO-VERIFIED-BUILD\plugin'

Use the plugin folder from the earlier successful clean build (0b011fb) or a newly compiled matching build. The script requires all three exact SHA-256 values recorded in VERIFICATION.md; it never modifies the installed game or plugin files. It creates a new release-output folder, refuses to overwrite it, and includes only explicitly allowlisted source files and license notices.

Outputs:

- latest.zip — installer draft: original three plugin files at ZIP root, full license, third-party notices and source access draft.
- SimBridge-v0.8.0-beta.1-source.zip — reviewed source, build and test tools, documentation and license files, excluding generated DLLs, logs and private files.
- repo.json — proposed GitHub-hosted feed; not usable while private. LastUpdate must be set to actual publication time before publishing.
- repo.local.json — loopback-only feed for private testing.
- icon.png — approved 128x128 lavender plugin icon, served by the private loopback feed.
- SHA256SUMS.txt — generated archive, feed and icon hashes.

Do not distribute these draft files publicly until the provenance, corresponding-source and privacy gates are closed. Packaging succeeded on Windows at 555da8f; private assets and local feed passed inspection and installation tests. See VERIFICATION.md for the historical draft hashes and results. Final archives must be regenerated after documentation changes. No GitHub Release or public live repository exists.

## Proposed custom repository entry

Based on the official Dalamud custom repository documentation:
https://dalamud.dev/plugin-publishing/custom-repositories/

The feed is an array with one stable-channel store entry, version 0.8.0.0 and API 15. The GitHub release tag identifies this as a beta; the custom repo is opt-in. We intentionally do not mark this as a separate testing-only channel or advertise a higher testing assembly version. Future runtime releases must increment the assembly/manifest/store version coherently. Changing a ZIP alone at the same version is not a reliable update test.

Planned public feed:
https://raw.githubusercontent.com/azazev/SimBridge/main/repo.json

Planned install/update asset:
https://github.com/azazev/SimBridge/releases/download/v0.8.0-beta.1/latest.zip

Planned public icon: https://raw.githubusercontent.com/azazev/SimBridge/main/assets/icon.png

These URLs are not live. Dalamud does not support authentication for custom repository feeds; a GitHub-hosted end-to-end test needs separately authorized public access.

## Private local installation test (after packaging review)

If a local Python 3 installation is already available, the draft feed can be served from release-output on loopback only (do not bind to all interfaces):

    python -m http.server 8765 --bind 127.0.0.1 --directory .\release-output

In a second window, check http://127.0.0.1:8765/repo.local.json, latest.zip and icon.png. The updated private feed points to the local icon; the public template points to the planned main-branch asset. Loopback HTTP was accepted by the tested client on November 24, 2026. Other client versions or configurations may differ; if rejected, stop and use the existing manual development-plugin verification route rather than exposing a private feed publicly or weakening security settings.

Before installer testing, back up the existing SimBridge configuration and disable/remove its development-plugin registration; never run two copies with the same internal identity. Add the loopback repo only if accepted, install SimBridge, check /simbridge and /simbridge status, verify enable/disable preference and clean reload, and start one fresh supported simulation. Test uninstall/reinstall and persistence with backups. Stop the server and remove the temporary custom repo afterward. Do not change upstream versions solely for this test.

The owner confirmed installation, preference persistence, Diamond Dust functional smoke testing and same-version reinstall. One intermittent Diamond Dust assignment discrepancy remains documented in COMPATIBILITY.md, accepted as an experimental-beta limitation; no runtime fix claimed. A same-version reinstall is not a new-version update or rollback test. Use a separately validated later version for a real update test; prior v0.7.3 is not a valid rollback target for BossMod 7.5.8.10.

## Before public activation

Resolve the open provenance/corresponding-source questions in PROVENANCE.md. Finalize SOURCE.md links/status and any required notices; regenerate the packages and checksums. Review every asset and final history. Obtain explicit merge approval; obtain separate approval for public repository visibility and release. Upload both binary and source assets, publish the approved tag/release and root repo.json in a coordinated order, verify anonymous HTTPS downloads and actual Dalamud installation. Do not advertise a feed before its assets are reachable.
