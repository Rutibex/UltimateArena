# Ultimate Arena — native C# edition

A Windows desktop arena shooter written in C#/.NET 8. The window, raw mouse input, OpenGL rendering and synthesized audio use Windows system APIs. No browser, JavaScript, Node.js, game engine package or external audiovisual assets are used.

This is an original implementation. It does not use the Perfect Dark or kkrieger engines. Arena geometry, robot and weapon assemblies, four materials, the bitmap font, sounds, movement, combat, navigation and networking are all defined in C# source.

## Play

The portable Windows distribution contains `UltimateArena.exe` and its bundled .NET runtime. Extract the entire archive and double-click the executable. Keep the accompanying DLLs beside it. No separately installed .NET runtime or SDK is required for the portable build.

The menu offers:

- **Enter the Arena:** local deathmatch against the selected bot count (default five).
- **Host Match:** starts an authoritative UDP host on port 27960 and connects your local player to it.
- **Join Match:** connects to the IPv4 address/hostname and port entered in the address field.

Click the callsign/address fields to edit them; Backspace erases and Enter finishes editing. Mouse sensitivity, volume, bot count and vsync can be changed in the menu. Settings are stored under `%LOCALAPPDATA%\UltimateArena\settings.json`. Escape releases mouse capture and opens the menu. The local match pauses in the menu; an online match continues.

The client targets Windows x64 with a working OpenGL driver. The dedicated host and self-tests also run on other systems with .NET 8. Linux graphics support is limited to the offscreen QA mode; this is not a Linux desktop client.

## Slice contents

One 48 × 48 metre arena with a central reactor, cover, two upper galleries, four staircases and two launch pads. Deathmatch ends at 20 frags or eight minutes; a new round follows after eight seconds. Death has a two-second respawn and brief spawn protection. All six weapons are available immediately for testing. Health, armor and ammunition pickups return after twelve seconds.

Bots navigate collision-derived walkable nodes, traverse stairs, choose weapons by range, chase around blocked sight lines, strafe and fight each other and human players. They currently do not plan dodge/jump-pad maneuvers.

| Slot | Weapon | Primary | Alternate |
| --- | --- | --- | --- |
| 1 | Kestrel | Accurate sidearm | Slower focused shot |
| 2 | Tempest | 900 RPM carbine | Faster, wider discharge |
| 3 | Breacher | Nine-pellet blast | Tighter sixteen-pellet volley; two ammo |
| 4 | Longbow | High-damage precision rail | Hold zoom |
| 5 | Comet | Explosive rocket | Guided rocket |
| 6 | Latch | Bouncing timed grenade | Sticky proximity explosive |

| Control | Action |
| --- | --- |
| WASD / mouse | Move / look |
| Space | Jump; holding repeats on landing |
| Shift | Dodge; holding repeats when cooldown permits |
| Ctrl | Slower movement, reduced spread |
| 1–6 / wheel | Select weapon |
| Left / right mouse | Fire / alternate |
| Tab | Scoreboard |
| Esc | Menu / resume |
| F or F11 | Toggle fullscreen during play |

Ctrl is a steady-aim stance. It does not change eye height or collision dimensions in this slice.

## Multiplayer

For a LAN match, one player selects **Host Match**. The other enters the host's LAN IPv4 address, for example `192.168.1.50:27960`, and selects **Join Match**. Allow UDP 27960 through the host's firewall for the intended network. The menu can host up to eight human participants plus up to ten configured bots.

For a headless host:

```sh
UltimateArena.exe --server --port 27960 --bots 5
```

For internet play, the host must be reachable on its chosen UDP port. Automatic NAT traversal, a relay, public lobby and matchmaking are not implemented. Real WAN latency/loss and two separate computers have not been playtested in this environment.

The host owns movement, ammo, hits, damage, pickups and scores. Clients send intent, use local movement prediction and replay unacknowledged inputs. Other players are visually smoothed. Simulation runs at 60 Hz and snapshots at 20 Hz. Binary datagrams are bounded to 1200 bytes and split into actor/projectile groups. Endpoint/session tokens, sequence checks, input validation, traffic limits, heartbeat and timeout handling are included. Historical hit rewind and reliable event delivery are not implemented; sounds/visual events can be lost under packet loss while subsequent state snapshots still correct scores and positions.

## Build from source

Install the .NET 8 SDK and open `UltimateArena.csproj` in Visual Studio, or run:

```sh
dotnet build -c Release
dotnet run -c Release
dotnet publish -c Release -r win-x64 --self-contained true -o artifacts/windows
```

`Build-Windows.cmd` runs the portable publish command. There are no NuGet package dependencies; publishing can download Microsoft's runtime/host packs. The SDK's normal restore can require access to NuGet even though the game references no third-party packages.

## Verify

```sh
dotnet run -c Release -- --self-test
```

The self-test executable runs 20 checks, including a real two-client UDP match. It requires no graphics or audio device. On Linux with Mesa EGL/OpenGL/GLU installed:

```sh
dotnet run -c Release -- --render-test renderer-check.ppm
```

That invokes the actual C# renderer in an offscreen OpenGL context and saves a frame. See `docs/VALIDATION.md` for what was and was not tested.

## Source map

- `Core/`: world, movement, collision, weapons, navigation and bot combat.
- `Network/`: binary protocol, authoritative host, client prediction and snapshots.
- `Graphics/`: immediate-mode OpenGL renderer, original material generation and bitmap UI.
- `Native/`: Windows window/input/audio bindings and Linux offscreen QA.
- `DesktopGame.cs`: desktop controls, menus, HUD and game loop.
- `Tests/`: dependency-free simulation/network checks.

The renderer intentionally uses a simple compatibility OpenGL pipeline for this small first slice. It does not yet provide modern shadowing, skeletal animation, a level editor or advanced material shaders. It is not Unreal Tournament feature parity.

Original code and generated assets: MIT license, Michael Robinson. Microsoft runtime components in the portable distribution retain their supplied license and third-party notices.
