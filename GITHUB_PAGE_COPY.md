# GitHub repository setup copy

**Suggested repository name:** `ultimate-arena`

**Description (paste into GitHub's About field):**

Fast-paced arena FPS prototype with six weapons, bots and multiplayer. Standalone C# Windows and WebGL/Node.js editions.

**Suggested topics:**

`arena-shooter`, `fps`, `game`, `csharp`, `dotnet`, `windows`, `webgl`, `javascript`, `nodejs`, `multiplayer`, `open-source`, `indie-game`

**Suggested release tag:** `v0.2.0` (Windows C# edition version 0.2.0; browser edition remains version 0.1.0).

**Suggested release title:** `Ultimate Arena: The Foundry — Prototype v0.2.0`

**GitHub setup notes:**

1. Create an empty repository and upload the **contents** of the `Ultimate-Arena-GitHub-Repository` ZIP into its root (not the repository's ZIP folder itself).
2. Keep the two downloadable player archives **out of Git history**; attach them as assets to a GitHub **Release** tagged `v0.2.0`.
3. Paste the accompanying `Ultimate-Arena-v0.2.0-GitHub-Release-Notes.md` into the Release description.
4. If helpful, set `docs/images/desktop-renderer-preview.png` as the repository social preview, but clearly label it as an offscreen renderer capture rather than a gameplay screenshot.
5. Verify the GitHub Actions build on the Windows runner; the provided Windows binary itself still needs manual Windows playtesting.

The MIT license is already included. The source snapshots and runtime distributions are separated, and third-party notices remain included.
