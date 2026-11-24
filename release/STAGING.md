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
- [ ] Confirm rights to license all contributions, any adapted-code notices and corresponding-source obligations for actual installed dependency releases; see PROVENANCE.md.
- [x] Stage verified original plugin/AnoBossBridge.json and plugin/AnoBossBridge.deps.json text metadata; preserve internal identity.
- [x] Add documented build-only, validation-only and explicit BossMod target options; preserve default historical .9/.10 validation.
- [x] Owner confirmed successful clean Windows compilation with warnings treated as errors, and groups 1–5 passed on commit 565afc7.
- [x] Correct remaining hardcoded historical target in group 6; both groups 6 and 10 share selected versions. Add ValidateOnly DLL prerequisite.
- [x] Clean corrected checkout `0b011fb` passed all 10 offline groups and all 11 installed compatibility groups on BossMod 7.5.8.10 / AnoMech 0.5.1.0; all three generated plugin files match the original tested package by SHA-256.
- [ ] Prepare final release archive with applicable licenses, notices and corresponding-source access; validate new-user installation, config persistence, update and rollback.
- [x] Verify new source commit uses azazev GitHub noreply identity.
- [x] Initial reachable main/staging history audit: 14 commits, 55 file changes; no obvious credential, personal path or private email patterns in retrieved complete patches. Verified author identities and initial automated GitHub committer; scanned original DLL printable strings.
- [ ] Final history/ref, PR discussion and release asset privacy audit after remaining edits (tags endpoint not accessible in current connector).
- [ ] Confirm custom repository `repo.json` URLs and updates with a live install.
- [ ] Receive explicit approval to make this repository or a release public.

The original DLL retains the internal name `AnoBossBridge` to preserve configuration identity. Do not rename DLL/manifest/deps files independently.

Build and package-file integrity gate passed for the documented Windows environment. Historical BossMod 7.5.8.9 was not retested in this run. Distribution and provenance gates remain OPEN. No runtime source or release DLL changes.

No `repo.json`, public release, or tag is active yet.
