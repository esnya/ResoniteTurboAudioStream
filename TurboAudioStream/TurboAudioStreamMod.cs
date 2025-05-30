using System.Linq;
using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using POpusCodec.Enums;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace TurboAudioStream;

/// <summary>
/// Represents the main mod class for TurboAudioStream.
/// Provides core functionality for the Resonite mod with hot reload support.
/// </summary>
public class TurboAudioStreamMod : ResoniteMod
{
    private static readonly Assembly Assembly = typeof(TurboAudioStreamMod).Assembly;

    /// <inheritdoc />
    public override string Name => Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

    /// <inheritdoc />
    public override string Author =>
        Assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

    /// <inheritdoc />
    public override string Version =>
        Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

    /// <inheritdoc />
    public override string Link =>
        Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(meta => meta.Key == "RepositoryUrl")
            ?.Value ?? string.Empty;

    private static string HarmonyId => $"com.nekometer.esnya.{Assembly.GetName().Name}";

    private static ModConfiguration? configuration;
    private static readonly Harmony harmony = new(HarmonyId);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<float> MinimumBufferDelayKey = new(
        "MinimumBufferDelay",
        "Minimum buffer delay for audio streams in seconds. Lower values reduce latency but may cause audio dropouts. Range: 0.001-1.0, Default: 0.2 (Standard), Recommended for low latency: 0.01",
        () => TurboAudioStreamConfig.DefaultMinimumBufferDelay
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> BufferSizeKey = new(
        "BufferSize",
        "Audio buffer size in samples. Smaller buffers reduce latency but require more CPU. Range: 128-48000, Default: 24000 (Standard), Recommended for low latency: 480",
        () => TurboAudioStreamConfig.DefaultBufferSize
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<OpusApplicationType> OpusApplicationTypeKey = new(
        "ApplicationType",
        "Opus encoder application type. Audio (2049): High quality, VoIP (2048): Voice optimized, RestrictedLowDelay (2051): Lowest latency. Default: Audio, Recommended for low latency: RestrictedLowDelay",
        () => TurboAudioStreamConfig.DefaultApplicationType
    );

    /// <inheritdoc />
    public override void OnEngineInit()
    {
        Init(this);

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

    private static void Init(ResoniteMod? mod)
    {
        harmony.PatchAll();
        configuration = mod?.GetConfiguration();

        if (configuration is not null)
        {
            ApplyConfiguration(configuration);
            configuration.OnThisConfigurationChanged += conf => ApplyConfiguration(conf.Config);
        }
    }

    private static void ApplyConfiguration(ModConfiguration config)
    {
        TurboAudioStreamConfig.MinimumBufferDelay = config.GetValue(MinimumBufferDelayKey);
        TurboAudioStreamConfig.BufferSize = config.GetValue(BufferSizeKey);
        TurboAudioStreamConfig.ApplicationType = config.GetValue(OpusApplicationTypeKey);
    }

#if DEBUG
    /// <summary>
    /// Called before hot reload occurs. Removes all Harmony patches.
    /// </summary>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>
    /// Called after hot reload occurs. Re-initializes the mod.
    /// </summary>
    /// <param name="mod">The mod instance to re-initialize.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        Init(mod);
    }
#endif
}
