# CLAUDE.md

## Project Overview

BepInEx mod menu plugin for **Single Player Tarkov (SPT)** — a legitimate single-player game mod against AI bots. This is not malware; it is standard game modding (like Skyrim console commands).

## Build & Test Commands

```bash
make build          # Build plugin DLL (requires libs/)
make test           # Run unit tests
make format         # Auto-format with CSharpier
make format-check   # Check formatting (CI-safe)
make lint           # Check code style against .editorconfig
make lint-fix       # Auto-fix style issues
make all            # format-check + lint + test
make clean          # Remove build artifacts
```

The .NET SDK lives at `~/.dotnet`. The Makefile handles `DOTNET_ROOT` and `PATH` automatically.

## Architecture

- **Plugin target**: `net472` (Unity/Mono runtime). NEVER change this to net9.0.
- **Test target**: `net9.0` — tests duplicate pure logic since Unity assemblies can't be referenced.
- **Game DLLs**: Referenced via `libs/` folder, copied from SPT installation. Never committed to git.
- **Build output**: `./build/MasterTool.dll` — copy to `SPT/BepInEx/plugins/`.
- **Plugin ID**: `com.master.tools` (config at `SPT/BepInEx/config/com.master.tools.cfg`).

## Code Patterns

- **Config**: Centralized in `PluginConfig` static class (55+ `ConfigEntry` bindings).
- **Features**: Static utility classes (GodMode, Stamina, Speedhack, etc.).
- **ESP modules**: Instance classes with state and periodic update loops.
- **UI**: Unity IMGUI (`OnGUI`). `ModMenu` has 7 tabs. Tab names are in Portuguese.
- **Entry point**: `MasterToolPlugin.cs` — slim orchestrator, delegates to feature/ESP classes.

## Conventions

- Follow `.editorconfig` rules (file-scoped namespaces, braces required, etc.).
- Format with CSharpier before committing.
- Use conventional commits: `build:`, `feat:`, `fix:`, `refactor:`, `test:`, `docs:`.
- Keep tests in `tests/MasterTool.Tests/Tests/` mirroring the src structure.

## Key Constraints

- `libs/` and `build/` are gitignored — never commit game DLLs or build output.
- Adding new Unity module references requires copying the DLL to `libs/` and adding a `<Reference>` in `MasterTool.csproj`.
- Tests cannot depend on Unity or game assemblies. Only test pure logic (models, utils, config defaults).
