# Public distribution

[SimBridge v0.8.0-beta.1](https://github.com/azazev/SimBridge/releases/tag/v0.8.0-beta.1) was published on November 25, 2026. Dalamud uses a single opt-in custom repository entry with assembly version `0.8.0.0`, internal identity `AnoBossBridge`, and API 15.

| Resource | Public location |
| --- | --- |
| Installer feed | https://raw.githubusercontent.com/azazev/SimBridge/main/repo.json |
| Install and update package | https://github.com/azazev/SimBridge/releases/download/v0.8.0-beta.1/latest.zip |
| Source archive and checksums | [Release assets](https://github.com/azazev/SimBridge/releases/tag/v0.8.0-beta.1) |

The feed uses the GitHub publication time (2026-11-25T07:03:25Z, Unix 1795590205) for `LastUpdate`. See [installation instructions](../INSTALLATION.md), [anonymous download verification](VERIFICATION.md#public-release-downloads), and the [source availability policy](SOURCE.md).

The loopback repository was used for private testing. Keep it out of public release assets and the root feed. Preserve the published beta packages and tag. Future runtime releases need a coherent assembly, manifest and feed version. Same-version reinstall was tested privately. Testing an actual update and rollback requires a second compatible version.

The [private packaging procedure at the release tag](https://github.com/azazev/SimBridge/blob/v0.8.0-beta.1/release/DISTRIBUTION.md) records how the original packages were prepared.
