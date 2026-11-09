# Contributing / bug reports

Both editions are prototypes; please keep reports specific to **browser (Node/WebGL)** or **desktop (Windows C#/.NET)**. They do not share code or networking protocols.

## Reproducing an issue

Include your OS and version, GPU/graphics driver (if relevant), edition and release version, whether you played solo or multiplayer, detailed reproduction steps, expected and actual behaviour, and any error message. For networking issues, mention the protocol and port (browser TCP 8080, desktop UDP 27960). Do not post credentials or private IP/public IP details you do not want to share.

## Local checks

- Browser: install Node.js 22+, run `cd browser && npm test`.
- Desktop: install the .NET 8 SDK, run `dotnet build -c Release` and `dotnet run -c Release -- --self-test` from `desktop/`. Interactive desktop play requires Windows.

Please avoid committing build output (`bin/`, `obj/`, `artifacts/`), runtime distributions, generated test outputs, IDE caches, node_modules, or local settings. Keep third-party license notices intact.
