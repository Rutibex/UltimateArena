# Validation record

## Passed

`node --test tests/*.test.js` — 14 checks:

1. Six original firing definitions extracted and linked.
2. Original spread distribution stance/focus multipliers.
3. Wall collision, jumping and landing.
4. Stair traversal to the three-metre gallery.
5. Navigation around the reactor and onto galleries.
6. Hitscan damage, scoring and respawn.
7. Solid geometry blocks shots.
8. Rocket detonation and radial damage.
9. Alternate pellet count, ammo cost, guided rockets and sticky grenades.
10. Health pickup consumption and timed availability.
11. Frag limit and next-round restart.
12. Ninety simulated seconds of five-bot combat, including movement, kills and finite/bounded state.
13. Two real WebSocket clients against a separate TCP server: join, navigation, shooting, kill, replicated score/event, ignored client position/score/ammo injection and disconnect.
14. Actual browser-client module exercised with minimal DOM and graphics stubs: practice entry, movement, weapon change, and connection to a real server with two bots.

Example observed network evidence: ALPHA scored one kill, both clients received kill event 7, server time approximately 2.7 seconds. Exact timing varies by scheduling. The clients were on the same machine using loopback TCP; this is a network-protocol integration test, not a WAN latency test.

The actual renderer's GLSL shaders were compiled and linked in an offscreen Mesa OpenGL ES 3 context. The renderer's generated draw commands were replayed at 1440 × 900 with the generated material textures. No GL errors were reported. The resulting frame was visually inspected. The optional harness is in `tools/qa/`; it requires Linux Mesa EGL and Python Pillow. This checks geometry/shaders, not browser UI composition or hardware performance.

## Not verified

- Full browser UI, pointer lock, fullscreen, mouse input and audible output in a running browser. No local browser binary was installed; the browser download failed and the remote browser could not reach the local host. The client-stub check is explicitly not a substitute for browser playtesting.
- Windows launcher execution. The launchers are included, but the build environment is Linux.
- Two different physical machines, real internet latency, packet loss, NAT traversal or public hosting.
- The original Perfect Dark native executable. It was not built or run and has not been made ROM-independent.
- Production security, long-duration host soak testing, low-end GPU performance or competitive balance.

The first user playtest should check: launch from the Windows script; enter practice; move up both galleries; switch/fire all six weapons and their alternate modes; confirm sound and mouse capture; then join the same host from two browser windows. A separate two-machine LAN/internet playtest is still needed.
