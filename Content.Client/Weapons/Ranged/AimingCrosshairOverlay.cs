using System.Numerics;
using Content.Client.CombatMode;
using Content.Client.Weapons.Ranged.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client.Weapons.Ranged;

public sealed class AimingCrosshairOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly IEyeManager _eye;
    private readonly IPlayerManager _player;
    private readonly CombatModeSystem _combatMode;
    private readonly SharedGunSystem _guns;
    private readonly ClientAimingSystem _aiming;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public AimingCrosshairOverlay(
        IEntityManager entManager,
        IEyeManager eye,
        IPlayerManager player,
        CombatModeSystem combatMode,
        SharedGunSystem guns,
        ClientAimingSystem aiming)
    {
        _entManager = entManager;
        _eye = eye;
        _player = player;
        _combatMode = combatMode;
        _guns = guns;
        _aiming = aiming;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return _player.LocalEntity is { } player &&
               _combatMode.IsInCombatMode(player) &&
               _guns.TryGetGun(player, out var gun) &&
               _entManager.HasComponent<GunComponent>(gun.Owner);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_aiming.TryGetLocalCrosshair(out var aimCoordinates, out _, out var visualSpread))
            return;

        if (aimCoordinates.MapId != args.MapId)
            return;

        var screenCoordinates = _eye.MapToScreen(aimCoordinates);
        if (screenCoordinates.Window == WindowId.Invalid)
            return;

        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var center = screenCoordinates.Position;
        var gap = (6f + visualSpread * EyeManager.PixelsPerMeter * 1.6f) * uiScale;
        var length = 8f * uiScale;
        var color = Color.White.WithAlpha(0.82f);
        var shadow = Color.Black.WithAlpha(0.72f);
        var shadowOffset = new Vector2(1f, 1f) * uiScale;

        DrawCrosshair(args.ScreenHandle, center + shadowOffset, gap, length, shadow);
        DrawCrosshair(args.ScreenHandle, center, gap, length, color);

        args.ScreenHandle.DrawCircle(center + shadowOffset, 1.4f * uiScale, shadow, false);
        args.ScreenHandle.DrawCircle(center, 1.4f * uiScale, color, false);
    }

    private static void DrawCrosshair(DrawingHandleScreen screen, Vector2 center, float gap, float length, Color color)
    {
        screen.DrawLine(center + new Vector2(-gap - length, 0f), center + new Vector2(-gap, 0f), color);
        screen.DrawLine(center + new Vector2(gap, 0f), center + new Vector2(gap + length, 0f), color);
        screen.DrawLine(center + new Vector2(0f, -gap - length), center + new Vector2(0f, -gap), color);
        screen.DrawLine(center + new Vector2(0f, gap), center + new Vector2(0f, gap + length), color);
    }
}
