# Integration boundary

This prototype is an asset and firing-data integration, not a native engine merger. The change of architecture was made to get a ROM-free runnable slice without pretending that the Perfect Dark executable's boot dependencies had been removed.

## Perfect Dark archive

Source archive: `perfect-dark-dabs-mod-dabs-mod.zip`; archive commit marker: `cc90f3635adb570352c55a7ca73ce7198a10769e`.

Selected original files are retained unchanged in `vendor/perfect-dark/`:

- `invitems.c`: six function definitions are parsed by `tools/extract_pd.py`.
- `bondgun.c`: reference for `bgunCalculatePlayerShotSpread`.
- `LICENSE`: archive's MIT notice.

Extracted symbols:

- `invfunc_falcon2_singleshot`
- `invfunc_cyclone_rapidfire`
- `invfunc_shotgun_single`
- `invfunc_sniperrifle_singleshot`
- `invfunc_rockerlauncher_shoot` (original spelling)
- `invfunc_devastator_shoot`

The runtime uses their damage values with explicit arena-specific scale factors, their hitscan spread values, the carbine's 900 RPM value, and the sidearm's recovery duration. `pdSpread` adapts the original `(RANDOMFRAC() - 0.5) * RANDOMFRAC()` distribution and stance/focus multipliers to angular offsets. Projection, balance, projectile behavior and secondary fire are new implementations, not exact Perfect Dark behavior.

No ROM textures, models, sound samples, stages, characters or animations are included. Extracted source is not evidence that the original game or its networking has been run. The original native build was not compiled: no ROM was supplied, and SDL2/build dependencies were absent.

## kkrieger archive

Source archive: `kkrieger-werkkzeug3-master.zip`.

`vendor/openktg/` contains unmodified `gentexture.cpp`, `gentexture.hpp`, `types.hpp`, `LICENSE` and `README.md` from `ktg/`.

`tools/materials.cpp` calls real OpenKTG `InitTexgen`, `GenTexture`, gradient initialization and multi-octave `Noise`. It then applies original panel seams, bolts and floor patterns. `tools/build_assets.py` converts the generated PPM bytes to PNG using Python's standard library. Four cached PNGs are shipped so players do not need a compiler.

The old DirectX renderer, Werkkzeug editor, mesh graph, V2 synthesizer and executable compressor are not integrated. Arena/robot/gun geometry and Web Audio synthesis are new code.

## New runtime

Movement, collision, bot navigation, combat resolution, rendering, audio, interface, prediction and WebSocket transport are new code. The host simulates fixed steps at 60 Hz. Each browser sends sanitized input intent. Full snapshots are sent at 20 Hz. Client prediction replays inputs after the last acknowledged sequence. Other actors follow received snapshots; interpolation, historical hit rewind and unreliable transport are future engineering work, not present features.

The server is one shared deathmatch room. It has no accounts, persistent stats, matchmaker or public relay.

## Completing the original native-engine objective

A true Perfect Dark-based standalone still requires a content-provider boundary covering boot tables, models, animations, text/fonts, audio and stages; a ROM-free native boot; generated models compatible with its skeleton/part conventions; and generated stage collision, portals and navigation. Those tasks are not solved by generating textures or by this independent runtime. This prototype's authored arena, material recipes and firing-data extraction can be retained while that work is done.
