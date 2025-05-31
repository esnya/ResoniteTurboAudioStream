# TurboAudioStream

A low-latency audio streaming optimization mod for [Resonite](https://resonite.com/) using [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).

## About

TurboAudioStream significantly reduces audio streaming latency in Resonite by patching the default audio stream settings. The mod applies optimized buffer configurations and Opus codec settings to minimize delay between your microphone input and what other users hear in the virtual world.

### Key Features

- **Reduced Latency**: Optimizes audio buffer settings to minimize streaming delay
- **Configurable Settings**: Customizable buffer size, delay, and Opus application type
- **Local User Only**: Only affects your outgoing audio stream, not incoming audio from others
- **Real-time Configuration**: Settings can be adjusted without restarting Resonite

### Configuration

The mod creates a configuration file that allows you to customize the audio settings:

- **MinimumBufferDelay** (0.001-1.0s): Audio buffer delay in seconds
  - Default: 0.2s
  - Recommended: 0.02s (significantly reduced latency)
  - Lower values reduce latency but may drop audio

- **BufferSize** (128-48000 samples): Buffer capacity in samples
  - Default: 24000 samples
  - Higher values improve stability; lower values reduce memory usage
  - **Note**: This setting does not affect latency

- **ApplicationType**: Opus encoder mode
  - Default: Audio
  - Recommended: RestrictedLowDelay (lowest latency mode)
  - Options: Audio (high quality), VoIP (voice optimized), RestrictedLowDelay (lowest latency)

- **EncoderDelay**: Opus frame delay for latency vs. quality (2.5ms - 60ms)
  - Default: Delay20ms (20ms)
  - Recommended: Delay20ms (balanced latency and quality)
  - **⚠️ WARNING**: Changing this setting may break the audio stream
  - Options: Delay2dot5ms (2.5ms), Delay5ms (5ms), Delay10ms (10ms), Delay20ms (20ms), Delay40ms (40ms), Delay60ms (60ms)

Configuration changes take effect immediately without requiring a restart.

## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
1. Place the [TurboAudioStream.dll](https://github.com/esnya/TurboAudioStream/releases/latest/download/TurboAudioStream.dll) into your `rml_mods` folder. This folder should be located at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a standard installation. You can create it if it's missing, or if you start the game once with the ResoniteModLoader installed it will create this folder for you.
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
