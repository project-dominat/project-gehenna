using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<float> WeaponAimRecoilKickMultiplier =
        CVarDef.Create("weapon.aim.recoil_kick_multiplier", 1.0f, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Zero keeps the per-weapon AimingComponent EyeOffsetSpeed; positive values override it.
    /// </summary>
    public static readonly CVarDef<float> WeaponAimAdsSmoothTime =
        CVarDef.Create("weapon.aim.ads_smooth_time", 0.0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Multiplier applied on top of GunAimValidation.DefaultTolerance.
    /// </summary>
    public static readonly CVarDef<float> WeaponAimEnvelopeTolerance =
        CVarDef.Create("weapon.aim.envelope_tolerance", 1.0f, CVar.SERVERONLY);

    public static readonly CVarDef<float> WeaponAimSwayMaxMultiplier =
        CVarDef.Create("weapon.aim.sway_max_multiplier", 1.0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> WeaponAimTelemetryEnabled =
        CVarDef.Create("weapon.aim.telemetry_enabled", true, CVar.SERVERONLY);

    public static readonly CVarDef<int> WeaponAimTelemetryThreshold =
        CVarDef.Create("weapon.aim.telemetry_threshold", 10, CVar.SERVERONLY);

    public static readonly CVarDef<float> WeaponAimTelemetryWindowSeconds =
        CVarDef.Create("weapon.aim.telemetry_window_seconds", 30.0f, CVar.SERVERONLY);

    public static readonly CVarDef<bool> WeaponAimDebugOverlay =
        CVarDef.Create("weapon.aim.debug_overlay", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
