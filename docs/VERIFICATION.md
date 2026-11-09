# Verification record and limitations

These are the **original supplied verification records**, not claims of a new end-to-end Windows or internet playtest.

**Browser edition:** The delivered documentation records **14 passing Node tests**, including simulated combat, local WebSocket networking, client logic with stubs, and an offscreen replay of WebGL drawing commands. It explicitly does **not** confirm a full browser playthrough (pointer lock, sound, fullscreen), Windows launcher behaviour, physical LAN machines, internet networking or hardware performance. Full details: [`browser/docs/VALIDATION.md`](../browser/docs/VALIDATION.md).

**Native C# edition:** The delivered documentation records **20 passing self-tests**, a two-client UDP host test over loopback, and successful offscreen OpenGL rendering. It **does not** confirm that the Windows desktop executable was run on Windows or tested with actual Windows input and sound hardware. It also notes that normal MSBuild project execution was not verified in the original build environment. Full details: [`desktop/docs/VALIDATION.md`](../desktop/docs/VALIDATION.md).

Before publishing as a polished desktop game, extract and launch the Windows package on a Windows x64 machine, confirm audio and raw mouse input, play a full solo round, and host/join from two actual computers. A similar physical browser and LAN test is recommended for the browser build.

The two repository preview images are **offscreen render captures**, not verified screenshots of interactive sessions.
