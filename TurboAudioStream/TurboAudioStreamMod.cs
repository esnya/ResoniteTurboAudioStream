using System.Linq;
using System.Reflection;
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
    public override string Name =>
        Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title
        ?? Assembly.GetName().Name
        ?? string.Empty;

    /// <inheritdoc />
    public override string Author =>
        Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? string.Empty;

    /// <inheritdoc />
    public override string Version =>
        Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetName().Version?.ToString() ?? "0.0.0";

    /// <inheritdoc />
    public override string Link =>
        Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(meta => meta.Key == "RepositoryUrl")
            ?.Value
        ?? string.Empty;

    private static ModConfiguration? configuration;

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

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnableOutgoingTuningPatchKey = new(
        "EnableOutgoingTuningPatch",
        "Enable the outgoing Opus tuning patch. Default: true",
        () => TurboAudioStreamConfig.DefaultEnableOutgoingTuningPatch
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnableBindRepairPatchKey = new(
        "EnableBindRepairPatch",
        "Enable bind repair for AudioStreamInterface -> AudioStreamController.Stream. Default: false",
        () => TurboAudioStreamConfig.DefaultEnableBindRepairPatch
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnableAsyncFreshnessGuardPatchKey = new(
        "EnableAsyncFreshnessGuardPatch",
        "Enable stale async stream decode suppression. Default: false",
        () => TurboAudioStreamConfig.DefaultEnableAsyncFreshnessGuardPatch
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnableStreamConfigDropLogPatchKey = new(
        "EnableStreamConfigDropLogPatch",
        "Enable logging for stock stream drop reasons. Default: false",
        () => TurboAudioStreamConfig.DefaultEnableStreamConfigDropLogPatch
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnableReceiverHeadroomPatchKey = new(
        "EnableReceiverHeadroomPatch",
        "Enable remote receiver playback headroom floors. Default: false",
        () => TurboAudioStreamConfig.DefaultEnableReceiverHeadroomPatch
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<bool> EnablePatchTriggerLoggingKey = new(
        "EnablePatchTriggerLogging",
        "Log only when one of this mod's runtime patches actually intervenes. Default: false",
        () => TurboAudioStreamConfig.DefaultEnablePatchTriggerLogging
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<float> PatchLogCooldownSecondsKey = new(
        "PatchLogCooldownSeconds",
        "Minimum number of seconds before the same patch trigger log is emitted again. Default: 5.0",
        () => TurboAudioStreamConfig.DefaultPatchLogCooldownSeconds
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<float> ReceiverMinimumBufferDelayFloorKey = new(
        "ReceiverMinimumBufferDelayFloor",
        "Floor applied to remote receiver minimum buffer delay when the receiver headroom patch is enabled. Default: 0.05",
        () => TurboAudioStreamConfig.DefaultReceiverMinimumBufferDelayFloor
    );

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> ReceiverBufferSizeFloorKey = new(
        "ReceiverBufferSizeFloor",
        "Floor applied to remote receiver buffer size when the receiver headroom patch is enabled. Default: 12000",
        () => TurboAudioStreamConfig.DefaultReceiverBufferSizeFloor
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
        configuration = mod?.GetConfiguration();

        if (configuration is not null)
        {
            ApplyConfiguration(configuration);
            configuration.OnThisConfigurationChanged += conf => ApplyConfiguration(conf.Config);
        }
        else
        {
            AudioPatchManager.Synchronize();
        }
    }

    private static void ApplyConfiguration(ModConfiguration config)
    {
        TurboAudioStreamConfig.MinimumBufferDelay = config.GetValue(MinimumBufferDelayKey);
        TurboAudioStreamConfig.BufferSize = config.GetValue(BufferSizeKey);
        TurboAudioStreamConfig.ApplicationType = config.GetValue(OpusApplicationTypeKey);
        TurboAudioStreamConfig.EncoderDelay = config.GetValue(EncoderDelayKey);
        TurboAudioStreamConfig.EnableOutgoingTuningPatch = config.GetValue(
            EnableOutgoingTuningPatchKey
        );
        TurboAudioStreamConfig.EnableBindRepairPatch = config.GetValue(EnableBindRepairPatchKey);
        TurboAudioStreamConfig.EnableAsyncFreshnessGuardPatch = config.GetValue(
            EnableAsyncFreshnessGuardPatchKey
        );
        TurboAudioStreamConfig.EnableStreamConfigDropLogPatch = config.GetValue(
            EnableStreamConfigDropLogPatchKey
        );
        TurboAudioStreamConfig.EnableReceiverHeadroomPatch = config.GetValue(
            EnableReceiverHeadroomPatchKey
        );
        TurboAudioStreamConfig.EnablePatchTriggerLogging = config.GetValue(
            EnablePatchTriggerLoggingKey
        );
        TurboAudioStreamConfig.PatchLogCooldownSeconds = config.GetValue(
            PatchLogCooldownSecondsKey
        );
        TurboAudioStreamConfig.ReceiverMinimumBufferDelayFloor = config.GetValue(
            ReceiverMinimumBufferDelayFloorKey
        );
        TurboAudioStreamConfig.ReceiverBufferSizeFloor = config.GetValue(
            ReceiverBufferSizeFloorKey
        );
        AudioPatchManager.Synchronize();
    }

#if DEBUG
    /// <summary>
    /// Called before hot reload occurs. Removes all Harmony patches.
    /// </summary>
    public static void BeforeHotReload()
    {
        AudioPatchManager.UnpatchAll();
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
