using System.Numerics;
using System.Linq;
using Content.Shared._Gehenna.Medical.Trauma;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.IoC;
using Robust.Shared.Maths;

namespace Content.Client._Gehenna.Medical.Trauma;

public sealed class GehennaBodyMapControl : Control
{
    private const float SourceSize = 360f;
    private readonly Dictionary<string, Texture> _textures = new();
    private List<GehennaTraumaScannerEntry> _wounds = new();

    public GehennaBodyMapControl()
    {
        IoCManager.InjectDependencies(this);

        MinSize = SetSize = new Vector2(220, 260);
        RectClipContent = true;

        var cache = IoCManager.Resolve<IResourceCache>();
        LoadTexture(cache, "head", "/Textures/_Gehenna/HealthDisplay/Human/Human_Male_Head.png");
        LoadTexture(cache, "torso", "/Textures/_Gehenna/HealthDisplay/Human/Human_Male_Torso.png");
        LoadTexture(cache, "leftArm", "/Textures/_Gehenna/HealthDisplay/Human/Human_Male_left_arm.png");
        LoadTexture(cache, "rightArm", "/Textures/_Gehenna/HealthDisplay/Human/Human_Male_right_arm.png");
        LoadTexture(cache, "leftLeg", "/Textures/_Gehenna/HealthDisplay/Human/Human_Male_left_leg.png");
        LoadTexture(cache, "rightLeg", "/Textures/_Gehenna/HealthDisplay/Human/Human_Male_right_leg.png");
        LoadTexture(cache, "bruise", "/Textures/_Gehenna/HealthDisplay/UI/StatusIcons/HDIcon_Bruise.png");
        LoadTexture(cache, "cut", "/Textures/_Gehenna/HealthDisplay/UI/StatusIcons/HDIcon_Cut.png");
        LoadTexture(cache, "burn", "/Textures/_Gehenna/HealthDisplay/UI/StatusIcons/HDIcon_Burn.png");
        LoadTexture(cache, "gunshot", "/Textures/_Gehenna/HealthDisplay/UI/StatusIcons/HDIcon_Gunshot.png");
        LoadTexture(cache, "stab", "/Textures/_Gehenna/HealthDisplay/UI/StatusIcons/HDIcon_Stab.png");
        LoadTexture(cache, "infection", "/Textures/_Gehenna/HealthDisplay/UI/StatusIcons/HDIcon_WoundInfection.png");
    }

    public void SetWounds(List<GehennaTraumaScannerEntry> wounds)
    {
        _wounds = wounds;
        InvalidateMeasure();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        handle.DrawRect(PixelSizeBox, Color.FromHex("#15191fcc"));

        var side = MathF.Min(PixelWidth, PixelHeight - 28);
        var bodyBox = UIBox2.FromDimensions(new Vector2((PixelWidth - side) / 2f, 4), new Vector2(side, side));

        DrawTexture(handle, "torso", bodyBox);
        DrawTexture(handle, "head", bodyBox);
        DrawTexture(handle, "leftArm", bodyBox);
        DrawTexture(handle, "rightArm", bodyBox);
        DrawTexture(handle, "leftLeg", bodyBox);
        DrawTexture(handle, "rightLeg", bodyBox);

        foreach (var zone in Enum.GetValues<GehennaBodyZone>())
        {
            var zoneWounds = _wounds.Where(wound => wound.Zone == zone).ToList();
            if (zoneWounds.Count == 0)
                continue;

            DrawZoneMarker(handle, bodyBox, zone, zoneWounds);
        }
    }

    private void DrawZoneMarker(
        DrawingHandleScreen handle,
        UIBox2 bodyBox,
        GehennaBodyZone zone,
        List<GehennaTraumaScannerEntry> wounds)
    {
        var zoneBox = ScaleBox(GetZoneBox(zone), bodyBox);
        var severity = wounds.Sum(wound => wound.Severity.Float());
        var color = GetZoneColor(wounds, severity);

        handle.DrawRect(zoneBox, color.WithAlpha(0.26f));

        var iconOrigin = GetIconOrigin(zone, bodyBox);
        var iconSize = new Vector2(18, 18);
        var drawn = 0;

        foreach (var wound in wounds.OrderByDescending(wound => wound.Severity.Float()).Take(3))
        {
            var icon = GetWoundIcon(wound);
            if (icon == null)
                continue;

            var iconBox = UIBox2.FromDimensions(iconOrigin + new Vector2(drawn * 15, 0), iconSize);
            handle.DrawRect(Enlarge(iconBox, 1), Color.Black.WithAlpha(0.65f));
            handle.DrawTextureRect(icon, iconBox);
            drawn++;
        }

        if (wounds.Any(wound => wound.State is GehennaWoundState.Rotting or GehennaWoundState.Septic) &&
            _textures.GetValueOrDefault("infection") is { } infection)
        {
            var iconBox = UIBox2.FromDimensions(iconOrigin + new Vector2(drawn * 15, 0), iconSize);
            handle.DrawRect(Enlarge(iconBox, 1), Color.Black.WithAlpha(0.65f));
            handle.DrawTextureRect(infection, iconBox);
        }
    }

    private Texture? GetWoundIcon(GehennaTraumaScannerEntry wound)
    {
        var key = wound.Type switch
        {
            GehennaTraumaType.Bruise => "bruise",
            GehennaTraumaType.Cut => "cut",
            GehennaTraumaType.Puncture => "stab",
            GehennaTraumaType.Gunshot => "gunshot",
            GehennaTraumaType.Burn => "burn",
            _ => "bruise",
        };

        return _textures.GetValueOrDefault(key);
    }

    private static UIBox2 GetZoneBox(GehennaBodyZone zone)
    {
        return zone switch
        {
            GehennaBodyZone.Head => new UIBox2(142, 27, 217, 92),
            GehennaBodyZone.Torso => new UIBox2(120, 92, 240, 218),
            GehennaBodyZone.LeftArm => new UIBox2(68, 96, 135, 225),
            GehennaBodyZone.RightArm => new UIBox2(225, 96, 292, 225),
            GehennaBodyZone.LeftLeg => new UIBox2(120, 210, 176, 340),
            GehennaBodyZone.RightLeg => new UIBox2(184, 210, 240, 340),
            _ => new UIBox2(120, 92, 240, 218),
        };
    }

    private static Vector2 GetIconOrigin(GehennaBodyZone zone, UIBox2 bodyBox)
    {
        var source = zone switch
        {
            GehennaBodyZone.Head => new Vector2(222, 62),
            GehennaBodyZone.Torso => new Vector2(244, 132),
            GehennaBodyZone.LeftArm => new Vector2(48, 142),
            GehennaBodyZone.RightArm => new Vector2(250, 142),
            GehennaBodyZone.LeftLeg => new Vector2(82, 246),
            GehennaBodyZone.RightLeg => new Vector2(220, 246),
            _ => new Vector2(244, 132),
        };

        return bodyBox.TopLeft + source / SourceSize * bodyBox.Size;
    }

    private static Color GetZoneColor(List<GehennaTraumaScannerEntry> wounds, float severity)
    {
        if (wounds.Any(wound => wound.State == GehennaWoundState.Septic))
            return Color.Red;

        if (wounds.Any(wound => wound.State == GehennaWoundState.Rotting))
            return Color.Orange;

        if (wounds.Any(wound => wound.Type == GehennaTraumaType.Burn))
            return Color.DarkOrange;

        if (severity >= 35)
            return Color.Red;

        if (severity >= 18)
            return Color.OrangeRed;

        return Color.DeepSkyBlue;
    }

    private void DrawTexture(DrawingHandleScreen handle, string key, UIBox2 bodyBox)
    {
        if (_textures.TryGetValue(key, out var texture))
            handle.DrawTextureRect(texture, bodyBox);
    }

    private static UIBox2 ScaleBox(UIBox2 source, UIBox2 target)
    {
        return new UIBox2(
            target.Left + source.Left / SourceSize * target.Width,
            target.Top + source.Top / SourceSize * target.Height,
            target.Left + source.Right / SourceSize * target.Width,
            target.Top + source.Bottom / SourceSize * target.Height);
    }

    private static UIBox2 Enlarge(UIBox2 box, float amount)
    {
        return new UIBox2(box.Left - amount, box.Top - amount, box.Right + amount, box.Bottom + amount);
    }

    private void LoadTexture(IResourceCache cache, string key, string path)
    {
        _textures[key] = cache.GetResource<TextureResource>(path);
    }
}
