using System.Linq;
using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaResoniteModTemplate;

/// <summary>
/// Represents the main mod class for EsnyaResoniteModTemplate.
/// Provides core functionality for the Resonite mod with hot reload support.
/// </summary>
public class EsnyaResoniteModTemplateMod : ResoniteMod
{
    private static readonly Assembly Assembly = typeof(EsnyaResoniteModTemplateMod).Assembly;

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

    /// <inheritdoc />
    public override void OnEngineInit()
    {
        Init(this);

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

    /// <summary>
    /// Initializes the mod by applying Harmony patches and loading configuration.
    /// </summary>
    /// <param name="mod">The mod instance to initialize.</param>
    private static void Init(ResoniteMod? mod)
    {
        harmony.PatchAll();
        configuration = mod?.GetConfiguration();
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
