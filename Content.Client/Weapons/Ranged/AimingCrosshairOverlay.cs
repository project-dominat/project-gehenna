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
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client.Weapons.Ranged;

public sealed class AimingCrosshairOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly IEyeManager _eye;
    private readonly IPlayerManager _player;
    private readonly CombatModeSystem _combatMode;
    private readonly SharedGunSystem _guns;
    private readonly ClientAimingSystem _aiming;
    private readonly IGameTiming _timing;

    private const float BaseGap = 6f;
    private const float BaseLength = 10f;
    private const float SpreadScale = 1.6f;
    private const float MinCrosshairRadius = 7f;
    private const float MaxCrosshairRadius = 52f;
    private const float MinLineAlpha = 0.62f;
    private const float MaxLineAlpha = 0.96f;
    private const float BaseLineWidth = 2.4f;
    private const float OutlineLinePadding = 2.4f;
    private const float CenterDotRadius = 2.2f;
    private const float CenterDotOutlineRadius = 3.8f;
    private const float FiringBloomThreshold = 0.08f;
    private const float ReloadCooldownMultiplier = 1.5f;
    private const float FiringImpactDecay = 5.5f;
    private const float FiringImpactBloomStep = 0.015f;

    private EntityUid? _lastGun;
    private EntityUid? _reloadGun;
    private float _lastBloom;
    private float _lastDrawTime;
    private float _drawTime;
    private float _firingImpact;
    private float _reloadProgress;
    private TimeSpan _reloadStarted;
    private TimeSpan _reloadEnds;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public AimingCrosshairOverlay(
        IEntityManager entManager,
        IEyeManager eye,
        IPlayerManager player,
        CombatModeSystem combatMode,
        SharedGunSystem guns,
        ClientAimingSystem aiming,
        IGameTiming timing)
    {
        _entManager = entManager;
        _eye = eye;
        _player = player;
        _combatMode = combatMode;
        _guns = guns;
        _aiming = aiming;
        _timing = timing;
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
        if (!_aiming.TryGetLocalCrosshair(out var aimCoordinates, out var gun, out var visualSpread))
            return;

        if (aimCoordinates.MapId != args.MapId)
            return;

        var screenCoordinates = _eye.MapToScreen(aimCoordinates);
        if (screenCoordinates.Window == WindowId.Invalid)
            return;

        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var center = screenCoordinates.Position;
        var state = GetState(gun.Owner, gun.Comp);
        var bloom = GetBloom(gun.Owner, gun.Comp);
        var ammoCount = _guns.GetAmmoCount(gun.Owner);
        var isReloading = state == CrosshairState.Reloading;
        var spreadRadius = BaseGap + MathF.Log(1f + visualSpread * EyeManager.PixelsPerMeter * SpreadScale) * 8f;
        var gap = Math.Clamp(spreadRadius, MinCrosshairRadius, MaxCrosshairRadius) * uiScale;
        var length = BaseLength * MathHelper.Lerp(1f, 0.72f, bloom) * uiScale;

        _drawTime = (float)_timing.CurTime.TotalSeconds;
        UpdateFiringImpact(gun.Owner, state, bloom, _drawTime);
        UpdateReloadProgress(gun.Owner, gun.Comp, isReloading);

        DrawCrosshair(args.ScreenHandle, center, gap, length, uiScale, bloom, isReloading, ammoCount);
    }

    private CrosshairState GetState(EntityUid uid, GunComponent gun)
    {
        if (IsReloading(uid, gun))
            return CrosshairState.Reloading;

        if (_guns.GetAmmoCount(uid) <= 0)
            return CrosshairState.Empty;

        if (GetBloom(uid, gun) > FiringBloomThreshold)
            return CrosshairState.Firing;

        if (_player.LocalEntity is { } player &&
            _entManager.TryGetComponent(player, out ActiveAimingComponent? active) &&
            active.Weapon == uid)
        {
            var stabilizeTime = WeaponSwayComponent.DefaultStabilizeTime;
            if (_entManager.TryGetComponent(uid, out WeaponSwayComponent? sway))
                stabilizeTime = sway.StabilizeTime;

            if (stabilizeTime > 0f &&
                (_timing.CurTime - active.StartedAt).TotalSeconds < stabilizeTime)
            {
                return CrosshairState.AdsStabilizing;
            }

            return CrosshairState.AdsStable;
        }

        return CrosshairState.HipFire;
    }

    private bool IsReloading(EntityUid uid, GunComponent gun)
    {
        var cooldown = gun.NextFire - _timing.CurTime;
        if (cooldown <= TimeSpan.Zero)
            return false;

        var fireRate = gun.BurstActivated || gun.SelectedMode == SelectiveFire.Burst
            ? gun.BurstFireRate
            : gun.FireRateModified;

        if (fireRate <= 0f)
            return false;

        var baseline = TimeSpan.FromSeconds(1f / fireRate * ReloadCooldownMultiplier);
        return cooldown > baseline;
    }

    private float GetBloom(EntityUid uid, GunComponent gun)
    {
        var bloom = 0f;

        if (_entManager.TryGetComponent(uid, out GunRecoilComponent? recoil))
        {
            var maxPenalty = recoil.MaxSwayPenalty > 0f
                ? recoil.MaxSwayPenalty
                : GunRecoilComponent.DefaultMaxSwayPenalty;

            bloom = maxPenalty > 0f
                ? Math.Clamp(recoil.CurrentSwayPenalty / maxPenalty, 0f, 1f)
                : 0f;
        }

        var angleRange = gun.MaxAngleModified.Theta - gun.MinAngleModified.Theta;
        if (angleRange > 0)
        {
            var angleBloom = (gun.CurrentAngle.Theta - gun.MinAngleModified.Theta) / angleRange;
            bloom = Math.Max(bloom, (float)Math.Clamp(angleBloom, 0, 1));
        }

        return bloom;
    }

    private void UpdateFiringImpact(EntityUid gun, CrosshairState state, float bloom, float now)
    {
        if (_lastGun != gun)
        {
            _lastGun = gun;
            _lastBloom = bloom;
            _lastDrawTime = now;
            _firingImpact = 0f;
            return;
        }

        var frameTime = Math.Clamp(now - _lastDrawTime, 0f, 0.1f);
        _firingImpact = MathF.Max(0f, _firingImpact - FiringImpactDecay * frameTime);

        if (state == CrosshairState.Firing && bloom > _lastBloom + FiringImpactBloomStep)
        {
            var impact = Math.Clamp((bloom - _lastBloom) * 3f, 0.22f, 0.6f);
            _firingImpact = Math.Clamp(_firingImpact + impact, 0f, 1f);
        }

        _lastBloom = bloom;
        _lastDrawTime = now;
    }

    private void UpdateReloadProgress(EntityUid gun, GunComponent component, bool isReloading)
    {
        if (!isReloading)
        {
            _reloadGun = null;
            _reloadProgress = 0f;
            return;
        }

        if (_reloadGun != gun || _timing.CurTime >= _reloadEnds || _timing.CurTime < _reloadStarted)
        {
            _reloadGun = gun;
            _reloadStarted = _timing.CurTime;
            _reloadEnds = component.NextFire;
        }

        var duration = (_reloadEnds - _reloadStarted).TotalSeconds;
        if (duration <= 0)
        {
            _reloadProgress = 1f;
            return;
        }

        _reloadProgress = Math.Clamp((float)((_timing.CurTime - _reloadStarted).TotalSeconds / duration), 0f, 1f);
    }

    private void DrawCrosshair(
        DrawingHandleScreen screen,
        Vector2 center,
        float gap,
        float length,
        float uiScale,
        float bloom,
        bool isReloading,
        int ammoCount)
    {
        var impactScale = 1f + _firingImpact * 0.35f;
        gap *= impactScale;
        length *= 1f + _firingImpact * 0.2f;
        gap = Math.Clamp(gap, MinCrosshairRadius * uiScale, MaxCrosshairRadius * uiScale);

        bloom = Math.Clamp(bloom, 0f, 1f);
        var lineAlpha = MathHelper.Lerp(MaxLineAlpha, MinLineAlpha, bloom);
        var color = GetBloomColor(bloom).WithAlpha(lineAlpha);
        var shadow = Color.Black.WithAlpha(0.84f);
        var shadowOffset = new Vector2(1f, 1f) * uiScale;
        var lineWidth = MathHelper.Lerp(BaseLineWidth, BaseLineWidth * 0.85f, bloom) * uiScale;
        var outlineWidth = lineWidth + OutlineLinePadding * uiScale;
        var jitterStrength = Math.Clamp((Math.Max(bloom, _firingImpact) - 0.45f) / 0.55f, 0f, 1f) * 1.4f * uiScale;

        Vector2 Jitter(float seed)
        {
            if (jitterStrength <= 0f)
                return Vector2.Zero;

            return new Vector2(
                MathF.Sin(_drawTime * 54.7f + seed) * jitterStrength,
                MathF.Cos(_drawTime * 47.3f + seed * 1.37f) * jitterStrength);
        }

        void DrawLine(Vector2 from, Vector2 to, Color lineColor, float seed, float width = 1f)
        {
            var jitter = Jitter(seed);
            DrawThickLine(screen, center + from + jitter, center + to + jitter, lineColor, width);
        }

        if (ammoCount <= 0)
        {
            var blink = 0.45f + MathF.Sin(_drawTime * 9f) * 0.2f;
            var emptyColor = Color.Red.WithAlpha(blink);
            var emptyShadow = Color.Black.WithAlpha(0.65f);
            var size = Math.Clamp((gap + length * 0.7f) * 0.75f, 9f * uiScale, 22f * uiScale);

            DrawThickLine(screen, center + shadowOffset + new Vector2(-size, -size), center + shadowOffset + new Vector2(size, size), emptyShadow, outlineWidth);
            DrawThickLine(screen, center + shadowOffset + new Vector2(size, -size), center + shadowOffset + new Vector2(-size, size), emptyShadow, outlineWidth);
            DrawThickLine(screen, center + new Vector2(-size, -size), center + new Vector2(size, size), emptyColor, lineWidth);
            DrawThickLine(screen, center + new Vector2(size, -size), center + new Vector2(-size, size), emptyColor, lineWidth);
            screen.DrawCircle(center, CenterDotOutlineRadius * uiScale, emptyShadow, true);
            screen.DrawCircle(center, CenterDotRadius * uiScale, emptyColor, true);
            return;
        }

        DrawLine(new Vector2(-gap - length, 0f) + shadowOffset, new Vector2(-gap, 0f) + shadowOffset, shadow, 1f, outlineWidth);
        DrawLine(new Vector2(gap, 0f) + shadowOffset, new Vector2(gap + length, 0f) + shadowOffset, shadow, 2f, outlineWidth);
        DrawLine(new Vector2(0f, -gap - length) + shadowOffset, new Vector2(0f, -gap) + shadowOffset, shadow, 3f, outlineWidth);
        DrawLine(new Vector2(0f, gap) + shadowOffset, new Vector2(0f, gap + length) + shadowOffset, shadow, 4f, outlineWidth);

        DrawLine(new Vector2(-gap - length, 0f), new Vector2(-gap, 0f), color, 5f, lineWidth);
        DrawLine(new Vector2(gap, 0f), new Vector2(gap + length, 0f), color, 6f, lineWidth);
        DrawLine(new Vector2(0f, -gap - length), new Vector2(0f, -gap), color, 7f, lineWidth);
        DrawLine(new Vector2(0f, gap), new Vector2(0f, gap + length), color, 8f, lineWidth);

        screen.DrawCircle(center + shadowOffset, CenterDotOutlineRadius * uiScale, shadow, true);
        screen.DrawCircle(center, CenterDotRadius * uiScale, color, true);
        screen.DrawCircle(center, (CenterDotOutlineRadius + 1.2f) * uiScale, Color.Black.WithAlpha(0.52f), false);

        if (!isReloading)
            return;

        var radius = Math.Clamp(gap + length * 0.65f, 10f * uiScale, MaxCrosshairRadius * uiScale);
        var segments = 28;
        var progressSegments = Math.Clamp((int)MathF.Ceiling(segments * _reloadProgress), 1, segments);
        var step = MathF.PI * 2f / segments;
        var rotation = _drawTime * 4f;
        var reloadColor = Color.Orange.WithAlpha(0.9f);

        for (var i = 0; i < progressSegments; i++)
        {
            var a = rotation - MathF.PI / 2f + i * step;
            var b = a + step * 0.75f;
            var from = center + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
            var to = center + new Vector2(MathF.Cos(b), MathF.Sin(b)) * radius;

            DrawThickLine(screen, from + shadowOffset, to + shadowOffset, shadow, outlineWidth);
            DrawThickLine(screen, from, to, reloadColor, lineWidth);
        }
    }

    private static void DrawThickLine(DrawingHandleScreen screen, Vector2 from, Vector2 to, Color color, float width)
    {
        if (width <= 1f)
        {
            screen.DrawLine(from, to, color);
            return;
        }

        var delta = to - from;
        if (delta.LengthSquared() <= 0.001f)
        {
            screen.DrawCircle(from, width * 0.5f, color, true);
            return;
        }

        var normal = new Vector2(-delta.Y, delta.X).Normalized();
        var steps = Math.Max(1, (int)MathF.Ceiling(width * 0.5f));

        for (var i = -steps; i <= steps; i++)
        {
            var offset = normal * i;
            screen.DrawLine(from + offset, to + offset, color);
        }
    }

    private static Color GetBloomColor(float bloom)
    {
        return bloom < 0.5f
            ? Color.InterpolateBetween(Color.White, Color.Yellow, bloom * 2f)
            : Color.InterpolateBetween(Color.Yellow, Color.Red, (bloom - 0.5f) * 2f);
    }

    private enum CrosshairState : byte
    {
        HipFire,
        AdsStabilizing,
        AdsStable,
        Firing,
        Reloading,
        Empty,
    }
}
