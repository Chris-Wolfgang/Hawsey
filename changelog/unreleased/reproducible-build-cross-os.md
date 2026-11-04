type: docs

The engine's assemblies and symbols now rebuild to identical bytes on Linux, macOS and Windows: `docs/REPRODUCIBLE-BUILD.md` drops the "rebuild on Windows" requirement for releases built from this change on, and says to install the manifest's exact SDK privately, because the PDB records the runtime the compiler ran on.
