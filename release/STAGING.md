# SimBridge beta staging status

This is a **private release-preparation branch**, not a published release.

- Proposed public beta: `v0.8.0-beta.1`
- Developer: `azazev`
- Validated baseline: SimBridge v0.8.0 (November 24, 2026)
- Tested package SHA-256: `6b464968bd7241a991103a87c3385f319f7a3741ef16e9d8c32375616d55b194`
- Live-validated dependencies: BossMod Reborn 7.5.8.10 and AnoMech 0.5.1.0, Dalamud API 15
- Earlier BossMod 7.5.8.9: validated offline, not claimed as a complete gameplay test
- Published: **No**

## Before public release

- [x] Owner approved the private-staging documentation drafts.
- [x] Stage all 29 source/test/tooling files and verify exact Git blob hashes against the validated source preview.
- [ ] Resolve whether the bridge's tight reflection integration or copied/adapted expressions trigger AnoMech AGPL obligations; initial static review is inconclusive.
- [x] Confirm upstream repository license texts: AnoMech AGPL-3.0 and BossMod Reborn BSD-3-Clause.
- [x] Owner selected AGPL-3.0-or-later for SimBridge's own contributions; staged full license text, upstream notices, BSD license and approved optional Ko-fi link.
- [x] Owner confirmed independent personal project and approved license for their own contributions.
- [x] Resolve published AnoMech 0.5.1.0 and BossMod Reborn 7.5.8.10 source tags and verify their license files.
- [x] Identify local Dalamud 15.0.4.1 and Lumina 7.7.0 reference source revisions from assembly metadata; verify upstream commits and license texts; record four owner-reported SHA-256 fingerprints.
- [ ] Resolve adapted-code notices, applicable combined-work and corresponding-source obligations and any necessary upstream binary matching; see PROVENANCE.md.
- [x] Stage verified original plugin/AnoBossBridge.json and plugin/AnoBossBridge.deps.json text metadata; preserve internal identity.
- [x] Add documented build-only, validation-only and explicit BossMod target options; preserve default historical .9/.10 validation.
- [x] Owner confirmed successful clean Windows compilation with warnings treated as errors, and groups 1–5 passed on commit 565afc7.
- [x] Correct remaining hardcoded historical target in group 6; both groups 6 and 10 share selected versions. Add ValidateOnly DLL prerequisite.
- [x] Clean corrected checkout `0b011fb` passed all 10 offline groups and all 11 installed compatibility groups on BossMod 7.5.8.10 / AnoMech 0.5.1.0; all three generated plugin files match the original tested package by SHA-256.
- [x] Stage private distribution preparation script, source allowlist, source index, and inactive repository JSON template.
- [x] Packaging executed on Windows from `555da8f`; all three payload hashes, both archive entry lists and four output checksums verified. Source archive matches that staging tree.
- [x] Private loopback repository, installer-based loading, enable preference persistence, Diamond Dust smoke test and same-version uninstall/reinstall passed.
- [x] Owner accepted one unreproduced Diamond Dust assignment mismatch as a documented experimental-beta limitation, not a resolved bug. Record in COMPATIBILITY.md and VERIFICATION.md; keep on development backlog.
- [ ] Regenerate final documentation/source assets after staging edits; recheck package integrity and final source snapshot.
- [ ] Actual version-to-version update/rollback has not been tested; defer to a separately versioned compatible build. Do not claim same-version reinstall as an update.
- [ ] Finalize all required corresponding-source access and notices before distributing publicly.
- [x] Verify new source commit uses azazev GitHub noreply identity.
- [x] Initial reachable main/staging history audit: 14 commits, 55 file changes; no obvious credential, personal path or private email patterns in retrieved complete patches. Verified author identities and initial automated GitHub committer; scanned original DLL printable strings.
- [ ] Final history/ref, PR discussion and release asset privacy audit after remaining edits (tags endpoint not accessible in current connector).
- [x] Confirmed Dalamud recognizes loopback custom repository and installs verified ZIP.
- [ ] Verify anonymous public HTTPS feed and installer asset downloads after explicit public-release approval; do not advertise before verification.
- [ ] Receive explicit approval to make this repository or a release public.

The original DLL retains the internal name `AnoBossBridge` to preserve configuration identity. Do not rename DLL/manifest/deps files independently.

Build, package-file integrity and private distribution validation passed for the documented Windows environment. Historical BossMod 7.5.8.9 was not retested. The intermittent Diamond Dust observation remains unresolved and accepted for the experimental beta by the owner. Final corresponding-source compliance, final asset/privacy review, public feed verification and explicit approvals remain open. No runtime source or release DLL changes.

No root `repo.json`, public release, or tag is active yet. The draft template in release/ is not a live feed; generated packages must remain private pending review.
