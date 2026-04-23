using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

// Gehenna-Edit: Crosshair customization
public sealed partial class CCVars
{
    public static readonly CVarDef<bool> CrosshairEnabled =
        CVarDef.Create("crosshair.enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<string> CrosshairColor =
        CVarDef.Create("crosshair.color", "#FFFFFFFF", CVar.CLIENTONLY | CVar.ARCHIVE);

    // Empty string = follow main crosshair color.
    public static readonly CVarDef<string> CrosshairSpreadRailColor =
        CVarDef.Create("crosshair.spread_rail_color", "", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> CrosshairOpacity =
        CVarDef.Create("crosshair.opacity", 1.0f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> CrosshairGap =
        CVarDef.Create("crosshair.gap", 6f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> CrosshairLength =
        CVarDef.Create("crosshair.length", 10f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> CrosshairLineWidth =
        CVarDef.Create("crosshair.line_width", 2.4f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> CrosshairCenterDot =
        CVarDef.Create("crosshair.center_dot", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> CrosshairOutline =
        CVarDef.Create("crosshair.outline", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> CrosshairHitFlash =
        CVarDef.Create("crosshair.hit_flash", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> CrosshairSpreadRail =
        CVarDef.Create("crosshair.spread_rail", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> CrosshairSpreadScale =
        CVarDef.Create("crosshair.spread_scale", 2.4f, CVar.CLIENTONLY | CVar.ARCHIVE);

    // Tint crosshair from white -> yellow -> red as spread (bloom) grows.
    public static readonly CVarDef<bool> CrosshairBloomTint =
        CVarDef.Create("crosshair.bloom_tint", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> CrosshairCenterDotRadius =
        CVarDef.Create("crosshair.center_dot_radius", 2.2f, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<float> CrosshairOutlineThickness =
        CVarDef.Create("crosshair.outline_thickness", 2.4f, CVar.CLIENTONLY | CVar.ARCHIVE);

    // Bullet-envelope marker style when spread indicator is on.
    public static readonly CVarDef<bool> CrosshairDynamicLineWidth =
        CVarDef.Create("crosshair.dynamic_line_width", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
