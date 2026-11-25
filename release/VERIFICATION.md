# SimBridge v0.8.0-beta.1 — build verification

## Public release downloads

On November 25, 2026, unauthenticated HTTPS GETs followed the public GitHub release redirects and returned HTTP 200 for all three assets. SHA-256 values computed from the downloaded bytes matched the approved values.

| Published file | SHA-256 |
| --- | --- |
| `latest.zip` | `4a86e9ea89d3071e16c931bc024d053e4ae02ba1b71c84d30548a4c7a5fc14a6` |
| `SHA256SUMS.txt` | `ca4cbf48655e959357b74d5cbd56034fd469e888e12316570e71ad4bd6bfcc53` |
| `SimBridge-v0.8.0-beta.1-source.zip` | `19e4145a7ee960587516b6d5ff841f8df6ed72d69630b84ae8958a70620ee8b0` |

The tests below describe the November 24 private staging build and installer. An in-game installation from the public feed is a separate check.


## Tested build

- Source commit: `0b011fb8bd1333154637ae0a422d2e80013bfb48`
- Windows .NET SDK 10.0.100, reference pack 10.0.0; Dalamud API 15.
- BossMod Reborn 7.5.8.10; AnoMech 0.5.1.0.
- Owner ran `./Build-SimBridge-v0.8.0.ps1 -BossModVersions '7.5.8.10'` from a fresh downloaded source checkout.
- Compilation passed with warnings treated as errors; all 10 offline test groups passed.
- All 11 installed compatibility groups passed (87 ms, offline reflection only).
- Historical .9 was not installed and was not retested in this run. Full historical test mode remains available.

The owner supplied console output, the validation log and SHA-256 results. Raw logs and local filesystem paths are deliberately excluded.

## Integrity

The original development archive was independently checked: SHA-256 `6b464968bd7241a991103a87c3385f319f7a3741ef16e9d8c32375616d55b194`. The three extracted original plugin files exactly match the hashes and sizes reported for the clean GitHub build.

| File | Bytes | SHA-256 |
| --- | ---: | --- |
| AnoBossBridge.dll | 189440 | 2ee6b505d985121a8c9f37c43eeec1b8ce32c36b909fa0a3710d8eff39aa5993 |
| AnoBossBridge.deps.json | 457 | 2bd145ab4e7c34da4dc5b080cf8bb3dc26ff995772bbc6424d17e77e05199c81 |
| AnoBossBridge.json | 645 | e307627c2253c59ea6989269d3e016156c86985a8d0bdb8e3460851c5676ec71 |

The runtime DLL is byte-identical to the previously live-tested v0.8.0 baseline. The clean checkout therefore demonstrated a matching rebuild in this environment, not fresh gameplay coverage, arbitrary future dependency compatibility, or a cross-platform reproducible build. Documentation/license files added after the tested commit do not change runtime source.

Release ZIP layout, license/corresponding-source inclusion, repository feed, installation/update/rollback and final publication approval are separate gates.

## Private distribution and installation validation — 2026-11-24

Tested distribution checkout: `555da8fd7d21a83441af5217daf2e147859f000d`. The owner rebuilt it on the same Windows/dependency environment: no compiler warnings, all 10 offline test groups passed, all 11 installed compatibility groups passed (53 ms). The packaging script verified all three original payload hashes, manifest identity and both ZIP entry lists.

The uploaded draft assets were independently inspected during the release review; their four reported SHA-256 values matched, archive CRCs and payload hashes passed, and the 49-file source tree matched the tested distribution commit. The binary ZIP contained the three original plugin files, full license, third-party notices, upstream BSD license and source access index.

| Draft asset | Bytes | SHA-256 |
| --- | ---: | --- |
| latest.zip | 68859 | ff9c5b2bab5d205f357a94ac1837d069d64475ee9eae7db2a0251e6a9ed6ee3c |
| repo.json | 1039 | 78a606753a79c910d9542ce7cc14700e2381aa0f9fe8fe2d212086d60b633585 |
| repo.local.json | 1122 | 58b40c618e9969f8fc03a5cbdb24a0c23bef57443616f94d16d7754087b389d8 |
| SimBridge-v0.8.0-beta.1-source.zip | 109735 | c7ea1935b6246c7403a12f437a45b8a48af142208f555cb2695df91e5cfcac37 |

These are **historical private test asset hashes**, not final public release asset hashes. Regenerate the final archives from the approved source snapshot after documentation/notice changes and record their new checksums.

| Private client test | Result |
| --- | --- |
| Loopback HTTP server and repository discovery | Passed; feed returned HTTP 200, Dalamud recognized the single entry |
| Installer display | Correct name, author, assembly version and description; default icon |
| Installer-based install/load | Passed; commands and idle armed status available |
| Configuration persistence | Passed; disabled bridge preference remained unchecked after plugin disable/re-enable |
| Diamond Dust M2, NA Partner Swap smoke test | Owner reports full clear and functional BossMod module |
| Same-version uninstall/reinstall | Passed for installation and plugin loading |
| First post-reinstall Diamond Dust observation | Owner reported marked AOE player receiving cone-bait indication; unresolved |
| Two subsequent Diamond Dust attempts after another reinstall | Earlier mismatch not reproduced; owner reports guidance functional. One attempt completed, the other was deliberately failed by the owner. Count both as functional tests, not two mechanic clears. |
| Cleanup | Diagnostic review reported module cleanup after the recorded attempts |
| Genuine version-to-version update/rollback | Not performed |

The owner explicitly accepted the isolated, unreproduced mismatch as an experimental-beta backlog item on November 24. This is not evidence that reinstalling fixes it, that both marker variations were explicitly tested, or that all guidance is correct. Do not claim a runtime fix. Raw diagnostics and screenshots are excluded for privacy.

Private distribution validation is complete in the tested environment. Public delivery and licensing gates remain open.


## Icon-aware private packaging — 2026-11-24

Checkout: `6cdb54b0ecf61c064b0d43c4fe04b4a306393605`. Owner output reports all three unchanged payload hashes, manifest identity, archive entry lists and icon integrity passed. The subsequent uploaded-asset review reported five matching checksums, 50 source entries and seven installer entries, with the source tree matching that checkout.

| Private draft file | SHA-256 |
| --- | --- |
| icon.png | 6ad4cf3e8554f2f7623316dc95f1387ffd7f2e4428002328c6ed762f477e7e65 |
| latest.zip | f379884dc2b675d824389193d40bf5806382d2129efcbcb1295aa56a1bceffa2 |
| repo.json | 5ac33e6e112efdbc3f5ae0de5af65d40fc0aacdec2e5280b839239ee795f7b72 |
| repo.local.json | f0d45c1b0b6ea46e14564eea695ee13c513d3fffe0a047be4a82495c4d792104 |
| SimBridge-v0.8.0-beta.1-source.zip | 79f998a7f1785db38a02351a28c0520e24ac908c186fa9bb4b732fc7b6bf6510 |

The loopback icon opened in a browser, but Dalamud retained the default icon even after restart. No confirmed icon request from Dalamud was recorded. Owner deferred this non-blocking cosmetic issue; no fix is claimed. These are historical private draft hashes, superseded by the final packaging run after documentation changes.
