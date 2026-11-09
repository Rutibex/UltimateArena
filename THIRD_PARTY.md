# Third-party notices and source acknowledgements

The original Ultimate Arena: The Foundry code and original assets by Michael Robinson are distributed under the repository's [MIT license](LICENSE).

## Browser edition: Perfect Dark decompilation reference

The `browser/vendor/perfect-dark` files are from [DabDavis/perfect-dark-dabs-mod](https://github.com/DabDavis/perfect-dark-dabs-mod), copyright 2022 Ryan Dwyer, under the preserved MIT license at `browser/vendor/perfect-dark/LICENSE`. The browser implementation uses data extracted from selected weapon definitions and an adapted shot-spread rule. The browser is **not** the original Perfect Dark engine, and no game ROM or ROM-derived models, textures, audio, or maps are distributed.

## Browser edition: OpenKTG

The `browser/vendor/openktg` sources (copyright 2007 Fabian “ryg” Giesen) are dedicated to the public domain, as documented in `browser/vendor/openktg/LICENSE` and original source notices. Original archive collection: [farbrausch/fr_public](https://github.com/farbrausch/fr_public); the provided source also credits [Kkrieger-Werkkzeug3](https://github.com/steven-schronk/Kkrieger-Werkkzeug3). The shipped browser material textures were generated with this texture-generation library and original material recipes. The game runtime does not depend on compiling it.

## Windows portable edition: Microsoft .NET

The Windows release ZIP ships the Microsoft .NET 8 self-contained Windows x64 runtime. Microsoft's runtime license and third-party acknowledgements are bundled in `DOTNET-LICENSE.txt` and `DOTNET-ThirdPartyNotices.txt`. Microsoft components **do not** use the game's MIT license.

The native C# edition does not incorporate Perfect Dark or kkrieger source code. The two editions are independent implementations and have incompatible network protocols.
