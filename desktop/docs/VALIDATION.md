# Verification record — C# edition

## Passed

The C# source compiles with the .NET 8 SDK's Roslyn 4.11 compiler, C# 12, nullable analysis, optimization and warnings treated as errors.

The compiled managed executable ran **20 self-tests, all passing**. The full output is in `test-results.txt`:

- Six weapon definitions.
- Movement against walls, jump/landing, dodge cooldown, stair climbing and jump-pad launch.
- Navigation around the reactor and onto the galleries.
- Hitscan kills, scoring and respawn; blocked shots; rocket splash.
- Alternate pellet count/ammunition, guided rockets and sticky projectiles.
- Armor absorption, pickup cooldowns and round restart.
- Ninety simulated seconds of five-bot combat: 60 deaths, finite positions and bounded ammunition/health.
- Deterministic procedural material generation.
- x64 Windows structure sizes for the window, pixel format and audio bindings.
- Binary serialization round trips, eight-actor packet size below 1200 bytes and malformed input rejection.
- A real UDP host and two independent UDP clients on loopback: join, movement/navigation, shooting, a kill, score/kill replication to both clients, and disconnect cleanup. Observed completion about 2.46 seconds, with a 2 ms reported local ping.

The actual C# renderer ran in a Mesa OpenGL 4.5 compatibility context through EGL 1.5. It generated the materials, rendered the arena, robot models, rocket-launcher view model and a QA overlay at 1440 × 900, and reported no GL errors. The frame was visually inspected. `native-renderer-preview.png` is that offscreen frame, not a Windows gameplay screenshot and not the complete in-game HUD.

A flattened, self-contained runtime layout was also executed on Linux with the same managed game and its headless self-tests. This verifies the packaging method without an installed system .NET runtime. The Windows x64 apphost was generated using Microsoft's HostModel, bound to `UltimateArena.dll`, given the application icon/resources, and structurally checked as a Windows x64 GUI executable.

The portable Windows runtime was downloaded from Microsoft's official .NET release metadata and its SHA-512 matched the metadata. The distribution bundles version 8.0.32 and the supplied runtime license/notices.

## Not verified

- The Windows executable has not been run on Windows in this environment. Native window creation, raw mouse capture, audible output, fullscreen switching and the complete interactive menu/HUD still require a Windows playtest.
- Two physical machines and actual internet latency, loss, NAT routing and firewall behavior were not tested. Loopback UDP is a real networking test but is not a WAN test.
- Performance on the user's GPU, competitive balance, long-running public hosting and production security have not been evaluated.
- The usual MSBuild project pipeline could not run in this environment because the host does not expose the process metadata that the .NET CLI/MSBuild expect. This is why the SDK's C# compiler and host packager were invoked directly. The normal `.csproj`, Windows publish script and SDK commands are supplied for local development; their MSBuild execution remains unverified here.

The first Windows check should be: extract all files; double-click `UltimateArena.exe`; enter solo; verify mouse capture/release and sound; reach both galleries; fire all six weapons and alternate modes; finish/respawn; then host/join using a second process or machine.
