using System.Linq;
using System.Reflection;
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
public sealed class TurboAudioStreamMod : ResoniteMod
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
        "Audio buffer delay in seconds. Lower reduces latency but may drop audio. Default: 0.2, recommended: 0.02",
        () => TurboAudioStreamConfig.DefaultMinimumBufferDelay
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> BufferSizeKey = new(
        "BufferSize",
        "Buffer capacity in samples. Higher values improve stability; lower values reduce memory. NOTICE: Does not affect latency. Default: 24000",
        () => TurboAudioStreamConfig.DefaultBufferSize
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<OpusApplicationType> OpusApplicationTypeKey = new(
        "ApplicationType",
        "Opus encoder mode: Audio, VoIP, RestrictedLowDelay. Default: Audio, recommended: RestrictedLowDelay",
        () => TurboAudioStreamConfig.DefaultApplicationType
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<Delay> EncoderDelayKey = new(
        "EncoderDelay",
        "Opus frame delay for latency vs. quality (2.5ms - 60ms). <b><i>WARNING: Changing this may break the audio stream.</i></b> Default: Delay20ms, recommended: Delay20ms",
        () => TurboAudioStreamConfig.DefaultEncoderDelay
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
        TurboAudioStreamConfig.EncoderDelay = config.GetValue(EncoderDelayKey);
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
