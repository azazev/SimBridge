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
- [ ] Finalize copyright/provenance review, choose the SimBridge license, and add required notices before public visibility.
- [x] Stage verified original plugin/AnoBossBridge.json and plugin/AnoBossBridge.deps.json text metadata; preserve internal identity.
- [x] Add documented build-only, validation-only and explicit BossMod target options; preserve default historical .9/.10 validation.
- [x] Owner confirmed successful clean Windows compilation with warnings treated as errors, and groups 1–5 passed on commit 565afc7.
- [x] Correct remaining hardcoded historical target in group 6; both groups 6 and 10 share selected versions. Add ValidateOnly DLL prerequisite.
- [ ] Rerun complete offline validation on the corrected checkout using BossMod 7.5.8.10.
- [ ] Confirm clean build, release packaging, configuration persistence and new-user installation.
- [x] Verify new source commit uses azazev GitHub noreply identity.
- [ ] Audit complete Git history and release files before public visibility.
- [ ] Confirm custom repository `repo.json` URLs and updates with a live install.
- [ ] Receive explicit approval to make this repository or a release public.

The original DLL retains the internal name `AnoBossBridge` to preserve configuration identity. Do not rename DLL/manifest/deps files independently.

Build gate is still OPEN: the owner's environment checks found all pinned prerequisites except historical BossMod 7.5.8.9. The first clean-checkout build succeeded; the test run stopped at group 6 due to a leftover .9 requirement. Corrected tooling is statically reviewed only; complete rerun and DLL comparison remain pending. No runtime source or release DLL changes.

No `repo.json`, public release, or tag is active yet.
