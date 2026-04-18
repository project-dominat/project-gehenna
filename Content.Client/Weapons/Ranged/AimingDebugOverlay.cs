using System.Globalization;
using System.Numerics;
using System.Text;
using Content.Client.Resources;
using Content.Client.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client.Weapons.Ranged;

public sealed class AimingDebugOverlay : Overlay
{
    private const string TextFontPath = "/Fonts/NotoSans/NotoSans-Regular.ttf";
    private const int TextFontSize = 12;
    private const float Margin = 12f;

    private readonly IEntityManager _entManager;
    private readonly IPlayerManager _player;
    private readonly SharedGunSystem _guns;
    private readonly ClientAimingSystem _aiming;
    private readonly Font _font;
    private readonly StringBuilder _text = new();

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public AimingDebugOverlay(
        IEntityManager entManager,
        IPlayerManager player,
        SharedGunSystem guns,
        ClientAimingSystem aiming,
        IResourceCache resourceCache)
    {
        _entManager = entManager;
        _player = player;
        _guns = guns;
        _aiming = aiming;
        _font = resourceCache.GetFont(TextFontPath, TextFontSize);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return TryGetLocalGun(out _);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!TryGetLocalGun(out var gun))
            return;

        _aiming.TryGetLocalAimDebugTelemetry(out var telemetry);
        var hasAimState = telemetry.InCombatMode && telemetry.RawTarget != null && telemetry.AimedTarget != null;

        _text.Clear();
        _text.Append("Bloom: ");
        _text.AppendLine(FormatBloom(gun.Owner));
        _text.Append("Sway offset: ");
        _text.AppendLine(hasAimState ? FormatVector(telemetry.SwayOffset) : "N/A");
        _text.Append("Recoil offset: ");
        _text.AppendLine(hasAimState ? FormatVector(telemetry.RecoilOffset) : "N/A");
        _text.Append("Visual spread: ");
        _text.AppendLine(FormatDegrees(gun.Comp.CurrentAngle.Degrees));
        _text.Append("Est. envelope radius: ");
        _text.AppendLine(FormatEstimatedEnvelope(telemetry.EstimatedEnvelopeRadius));
        _text.Append("Raw target: ");
        _text.AppendLine(hasAimState ? FormatCoordinates(telemetry.RawTarget) : "N/A");
        _text.Append("Aimed target: ");
        _text.AppendLine(hasAimState ? FormatCoordinates(telemetry.AimedTarget) : "N/A");
        _text.Append("Ping-comp: (pending Phase C)");

        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var position = new Vector2(args.ViewportBounds.Left + Margin * uiScale, args.ViewportBounds.Top + Margin * uiScale);
        var shadowOffset = new Vector2(1f, 1f) * uiScale;
        var text = _text.ToString();

        args.ScreenHandle.DrawString(_font, position + shadowOffset, text, uiScale, Color.Black.WithAlpha(0.76f));
        args.ScreenHandle.DrawString(_font, position, text, uiScale, Color.White.WithAlpha(0.96f));
    }

    private bool TryGetLocalGun(out Entity<GunComponent> gun)
    {
        gun = default;

        return _player.LocalEntity is { } player &&
               _guns.TryGetGun(player, out gun) &&
               _entManager.HasComponent<GunComponent>(gun.Owner);
    }

    private string FormatBloom(EntityUid weapon)
    {
        if (!_entManager.TryGetComponent(weapon, out GunRecoilComponent? recoil))
            return "N/A";

        return recoil.CurrentSwayPenalty.ToString("F2", CultureInfo.InvariantCulture);
    }

    private static string FormatVector(Vector2? vector)
    {
        if (vector is not { } value)
            return "N/A";

        return value.X.ToString("F2", CultureInfo.InvariantCulture) +
               ", " +
               value.Y.ToString("F2", CultureInfo.InvariantCulture);
    }

    private static string FormatCoordinates(MapCoordinates? coordinates)
    {
        if (coordinates is not { } value)
            return "N/A";

        return "(" +
               value.Position.X.ToString("F2", CultureInfo.InvariantCulture) +
               ", " +
               value.Position.Y.ToString("F2", CultureInfo.InvariantCulture) +
               ") map:" +
               value.MapId.ToString();
    }

    private static string FormatDegrees(double degrees)
    {
        return degrees.ToString("F1", CultureInfo.InvariantCulture) + "\u00B0";
    }

    private static string FormatEstimatedEnvelope(float? radius)
    {
        return radius is { } value
            ? "~" + value.ToString("F2", CultureInfo.InvariantCulture)
            : "N/A";
    }
}
