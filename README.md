# EsnyaResoniteModTemplate

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/).

## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
1. Place the [EsnyaResoniteModTemplate.dll](https://github.com/esnya/EsnyaResoniteModTemplate/releases/latest/download/EsnyaResoniteModTemplate.dll) into your `rml_mods` folder. This folder should be located at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a standard installation. You can create it if it's missing, or if you start the game once with the ResoniteModLoader installed it will create this folder for you.
1. Launch the game. If you want to check that the mod is working you can check your Resonite logs.

## Development

### Requirements

- .NET Framework 4.7.2 or later
- Visual Studio 2022 or Visual Studio Code
- [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) for hot reloading during development (optional but recommended)
- dotnet SDK (Recommended version: 9.0 or later)

### Installation for Development

1. Clone this repository
2. Run `dotnet tool restore` to install local tools (e.g., csharpier).
3. Set up your Resonite installation path:
   - The project will automatically detect common Steam installation paths
   - Alternatively, set the `ResonitePath` property when building: `dotnet build -p:ResonitePath="Path\To\Your\Resonite"`
4. Build the project: `dotnet build`

### Install to `rml_mods` Directory (and `rml_mods/HotReloadMods`)

The project includes a custom target **Install** for development convenience:

```bash
dotnet build -t:Install
```

### Hot Reload Development

For hot reload development with DEBUG builds, ensure you have [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) installed in your Resonite installation.
