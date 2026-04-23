using System.Numerics;
using Content.Client.CombatMode;
using Content.Client.Weapons.Ranged.Systems;
using Content.Shared.CCVar;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client.Weapons.Ranged;

public sealed class AimingCrosshairOverlay : Overlay
{
    private readonly IEntityManager _entManager;
    private readonly IEyeManager _eye;
    private readonly IInputManager _input;
    private readonly IPlayerManager _player;
    private readonly CombatModeSystem _combatMode;
    private readonly SharedGunSystem _guns;
    private readonly ClientAimingSystem _aiming;
    private readonly SharedTransformSystem _transform;
    private readonly IGameTiming _timing;
    // Gehenna-Edit: settings-driven crosshair
    private readonly IConfigurationManager _cfg;

    private const float MaxCrosshairRadius = 150f;
    private const float MinLineAlpha = 0.62f;
    private const float MaxLineAlpha = 0.96f;
    private const float CenterDotOutlineBonus = 1.6f;
    private const float FiringBloomThreshold = 0.08f;
    private const float ReloadCooldownMultiplier = 1.5f;
    private const float FiringImpactDecay = 4.2f;
    private const float FiringImpactBloomStep = 0.008f;
    // Gehenna edit - minimum visible gap between end caps at zero spread
    private const float MinLateralGapTiles = 0.75f;
    // Gehenna edit - visual scale-down so displayed spread matches real bullet envelope more closely (~1.65x reduction)
    private const float SpreadRailDisplayScale = 0.62f;
    // Gehenna edit - hit flash: linear decay, ~150ms to zero
    private const float HitFlashDecay = 16.6f;

    private float _hitFlashIntensity;
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
        IInputManager input,
        IPlayerManager player,
        CombatModeSystem combatMode,
        SharedGunSystem guns,
        ClientAimingSystem aiming,
        SharedTransformSystem transform,
        IGameTiming timing,
        IConfigurationManager cfg)
    {
        _entManager = entManager;
        _eye = eye;
        _input = input;
        _player = player;
        _combatMode = combatMode;
        _guns = guns;
        _aiming = aiming;
        _transform = transform;
        _timing = timing;
        _cfg = cfg;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        // Gehenna-Edit: early-out when player disabled the crosshair
        if (!_cfg.GetCVar(CCVars.CrosshairEnabled))
            return false;

        return _aiming.ShouldDrawLocalCrosshair(out _, out _);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_aiming.TryGetLocalCrosshair(out var center, out var gun, out var visualSpread))
            return;

        var mouseWindow = _input.MouseScreenPosition.Window;
        if (mouseWindow == WindowId.Invalid)
            return;

        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var state = GetState(gun.Owner, gun.Comp);
        var bloom = GetBloom(gun.Owner, gun.Comp);
        var ammoCount = _guns.GetAmmoCount(gun.Owner);
        var isReloading = state == CrosshairState.Reloading;

        // Gehenna-Edit: resolve tunables from config
        var baseGap = _cfg.GetCVar(CCVars.CrosshairGap);
        var baseLength = _cfg.GetCVar(CCVars.CrosshairLength);
        var spreadScale = _cfg.GetCVar(CCVars.CrosshairSpreadScale);
        var spreadRailEnabled = _cfg.GetCVar(CCVars.CrosshairSpreadRail);
        var hitFlashEnabled = _cfg.GetCVar(CCVars.CrosshairHitFlash);

        // Gehenna edit start - render the real lateral bullet envelope instead of a small abstract bloom
        var hasSpreadRail = false;
        Vector2 railStart = default, railEnd = default;
        float railRadiusPixels = 0f;
        float envelopeSpread = 0f;
        if (spreadRailEnabled)
        {
            hasSpreadRail = TryGetLateralSpreadRail(
                center,
                out railStart,
                out railEnd,
                out railRadiusPixels,
                out envelopeSpread);
        }
        var displaySpread = Math.Max(visualSpread, envelopeSpread);
        var spreadRadius = baseGap + MathF.Min(displaySpread * EyeManager.PixelsPerMeter * spreadScale, MaxCrosshairRadius - baseGap);
        // Gehenna edit end
        // Gehenna-Edit: honour the configured base gap as the real minimum
        var gap = Math.Clamp(spreadRadius, baseGap, MaxCrosshairRadius) * uiScale;
        var length = baseLength * MathHelper.Lerp(1f, 0.72f, bloom) * uiScale;

        _drawTime = (float)_timing.CurTime.TotalSeconds;
        UpdateFiringImpact(gun.Owner, state, bloom, _drawTime, hitFlashEnabled);
        UpdateReloadProgress(gun.Owner, gun.Comp, isReloading);

        if (hasSpreadRail)
            DrawLateralSpreadRail(args.ScreenHandle, railStart, railEnd, center, railRadiusPixels, uiScale, bloom, gap, length);

        DrawCrosshair(args.ScreenHandle, center, gap, length, uiScale, bloom, ammoCount);
    }

    public void NotifyHit()
    {
        // Gehenna-Edit: respect the hit-flash toggle
        if (!_cfg.GetCVar(CCVars.CrosshairHitFlash))
            return;

        _hitFlashIntensity = Math.Max(_hitFlashIntensity, 1.0f);
    }

    private Color ApplyBloomTint(Color baseColor, float bloom)
    {
        if (!_cfg.GetCVar(CCVars.CrosshairBloomTint))
            return baseColor;

        bloom = Math.Clamp(bloom, 0f, 1f);
        if (bloom <= 0f)
            return baseColor;

        // Multiplicatively push the user color through white -> yellow -> red as bloom rises.
        var bloomColor = GetBloomColor(bloom);
        return new Color(
            baseColor.R * bloomColor.R,
            baseColor.G * bloomColor.G,
            baseColor.B * bloomColor.B,
            baseColor.A);
    }

    private Color GetSpreadRailBaseColor(float bloom)
    {
        var colorString = _cfg.GetCVar(CCVars.CrosshairSpreadRailColor);
        Color baseColor;
        if (!string.IsNullOrEmpty(colorString) && Color.TryFromHex(colorString) is { } railColor)
            baseColor = railColor;
        else
            baseColor = Color.TryFromHex(_cfg.GetCVar(CCVars.CrosshairColor)) ?? Color.White;

        return ApplyBloomTint(baseColor, bloom);
    }

    // Gehenna edit start - lateral-only spread visualization
    private bool TryGetLateralSpreadRail(
        Vector2 fallbackCenter,
        out Vector2 start,
        out Vector2 end,
        out float radiusPixels,
        out float envelopeSpread)
    {
        start = default;
        end = default;
        radiusPixels = 0f;
        envelopeSpread = 0f;

        if (!_aiming.TryGetLocalAimDebugTelemetry(out var telemetry) ||
            telemetry.RawTarget is not { } rawTarget ||
            telemetry.EstimatedEnvelopeRadius is not { } envelopeRadius ||
            envelopeRadius <= 0f)
        {
            return false;
        }

        var playerCoordinates = _transform.GetMapCoordinates(telemetry.Player);
        if (playerCoordinates.MapId == MapId.Nullspace ||
            rawTarget.MapId == MapId.Nullspace ||
            playerCoordinates.MapId != rawTarget.MapId)
        {
            return false;
        }

        Vector2? fallbackDirection = null;
        if (telemetry.Weapon is { } weapon &&
            _entManager.TryGetComponent(weapon, out GunComponent? gun))
        {
            fallbackDirection = GetFallbackAimDirection(telemetry.Player, gun);
        }

        if (!GunAimValidation.TryGetAimAxes(
                playerCoordinates.Position,
                rawTarget.Position,
                fallbackDirection,
                out _,
                out var lateralAxis))
        {
            return false;
        }

        var displayRadius = MathF.Max(envelopeRadius, MinLateralGapTiles) * SpreadRailDisplayScale;
        var startMap = new MapCoordinates(rawTarget.Position - lateralAxis * displayRadius, rawTarget.MapId);
        var endMap = new MapCoordinates(rawTarget.Position + lateralAxis * displayRadius, rawTarget.MapId);
        var centerMap = rawTarget;

        var startScreen = _eye.MapToScreen(startMap);
        var endScreen = _eye.MapToScreen(endMap);
        var centerScreen = _eye.MapToScreen(centerMap);

        if (startScreen.Window == WindowId.Invalid ||
            endScreen.Window == WindowId.Invalid ||
            centerScreen.Window == WindowId.Invalid ||
            startScreen.Window != endScreen.Window ||
            startScreen.Window != centerScreen.Window)
        {
            return false;
        }

        start = startScreen.Position;
        end = endScreen.Position;
        radiusPixels = MathF.Max((start - centerScreen.Position).Length(), (end - centerScreen.Position).Length());
        envelopeSpread = radiusPixels / MathF.Max(EyeManager.PixelsPerMeter, 1f);

        if ((fallbackCenter - centerScreen.Position).LengthSquared() > 4f)
        {
            var correction = fallbackCenter - centerScreen.Position;
            start += correction;
            end += correction;
        }

        return true;
    }

    private Vector2 GetFallbackAimDirection(EntityUid player, GunComponent gun)
    {
        var direction = gun.DefaultDirection;
        if (!float.IsFinite(direction.X) ||
            !float.IsFinite(direction.Y) ||
            direction.LengthSquared() <= GunAimValidation.AimAxisEpsilon)
        {
            direction = Vector2.UnitX;
        }

        direction = direction.Normalized();
        var rotated = _transform.GetWorldRotation(player).RotateVec(direction);
        if (!float.IsFinite(rotated.X) ||
            !float.IsFinite(rotated.Y) ||
            rotated.LengthSquared() <= GunAimValidation.AimAxisEpsilon)
        {
            return Vector2.UnitX;
        }

        return rotated.Normalized();
    }

    private void DrawLateralSpreadRail(
        DrawingHandleScreen screen,
        Vector2 start,
        Vector2 end,
        Vector2 center,
        float radiusPixels,
        float uiScale,
        float bloom,
        float gap,
        float armLength)
    {
        var delta = end - start;
        if (delta.LengthSquared() <= 0.001f)
            return;

        // Gehenna-Edit: pull settings
        var outlineEnabled = _cfg.GetCVar(CCVars.CrosshairOutline);
        var centerDotEnabled = _cfg.GetCVar(CCVars.CrosshairCenterDot);
        var hitFlashEnabled = _cfg.GetCVar(CCVars.CrosshairHitFlash);
        var opacity = Math.Clamp(_cfg.GetCVar(CCVars.CrosshairOpacity), 0f, 1f);
        var baseLineWidth = _cfg.GetCVar(CCVars.CrosshairLineWidth);
        var baseGap = _cfg.GetCVar(CCVars.CrosshairGap);
        var outlineThickness = _cfg.GetCVar(CCVars.CrosshairOutlineThickness);
        var centerDotRadius = _cfg.GetCVar(CCVars.CrosshairCenterDotRadius);
        var dynamicLineWidth = _cfg.GetCVar(CCVars.CrosshairDynamicLineWidth);

        var axis = delta.Normalized();
        var capAlpha = MathHelper.Lerp(0.92f, 0.75f, Math.Clamp(bloom, 0f, 1f));
        var railBase = GetSpreadRailBaseColor(bloom);
        var cap = railBase.WithAlpha(Math.Clamp(capAlpha * opacity, 0f, 1f));
        if (hitFlashEnabled && _hitFlashIntensity > 0f)
            cap = Color.InterpolateBetween(cap, Color.White.WithAlpha(Math.Clamp(capAlpha * opacity, 0f, 1f)), _hitFlashIntensity);
        var shadow = Color.Black.WithAlpha(Math.Clamp(0.82f * opacity, 0f, 1f));
        var lineWidth = dynamicLineWidth
            ? MathHelper.Lerp(baseLineWidth * 0.7f, baseLineWidth * 1.4f, Math.Clamp(bloom, 0f, 1f)) * uiScale
            : baseLineWidth * uiScale;
        var outlineWidth = lineWidth + outlineThickness * uiScale;
        var capLength = Math.Clamp(radiusPixels * 0.05f, 6f * uiScale, 8f * uiScale);
        var shadowOffset = new Vector2(1f, 1f) * uiScale;

        DrawCap(start);
        DrawCap(end);

        // Center crosshair: 4 separate arms with dynamic inner gap + center dot
        // Inner gap expands with bloom so the crosshair breathes with firing/movement
        var innerGap = Math.Clamp(gap * 0.18f, baseGap * uiScale, baseGap * 3f * uiScale);
        var arm = armLength;

        // Shadow pass
        if (outlineEnabled)
        {
            DrawThickLine(screen, center + shadowOffset + new Vector2(innerGap, 0f), center + shadowOffset + new Vector2(innerGap + arm, 0f), shadow, outlineWidth);
            DrawThickLine(screen, center + shadowOffset - new Vector2(innerGap, 0f), center + shadowOffset - new Vector2(innerGap + arm, 0f), shadow, outlineWidth);
            DrawThickLine(screen, center + shadowOffset + new Vector2(0f, innerGap), center + shadowOffset + new Vector2(0f, innerGap + arm), shadow, outlineWidth);
            DrawThickLine(screen, center + shadowOffset - new Vector2(0f, innerGap), center + shadowOffset - new Vector2(0f, innerGap + arm), shadow, outlineWidth);
            if (centerDotEnabled)
                screen.DrawCircle(center + shadowOffset, (centerDotRadius + CenterDotOutlineBonus) * uiScale, shadow, true);
        }
        // Color pass
        DrawThickLine(screen, center + new Vector2(innerGap, 0f), center + new Vector2(innerGap + arm, 0f), cap, lineWidth);
        DrawThickLine(screen, center - new Vector2(innerGap, 0f), center - new Vector2(innerGap + arm, 0f), cap, lineWidth);
        DrawThickLine(screen, center + new Vector2(0f, innerGap), center + new Vector2(0f, innerGap + arm), cap, lineWidth);
        DrawThickLine(screen, center - new Vector2(0f, innerGap), center - new Vector2(0f, innerGap + arm), cap, lineWidth);
        if (centerDotEnabled)
            screen.DrawCircle(center, centerDotRadius * uiScale, cap, true);

        void DrawCap(Vector2 position)
        {
            if (outlineEnabled)
                DrawThickLine(screen, position + shadowOffset - axis * capLength, position + shadowOffset + axis * capLength, shadow, outlineWidth);
            DrawThickLine(screen, position - axis * capLength, position + axis * capLength, cap, lineWidth);
        }
    }
    // Gehenna edit end

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

    private void UpdateFiringImpact(EntityUid gun, CrosshairState state, float bloom, float now, bool hitFlashEnabled)
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
        // Gehenna-Edit: skip hit-flash decay when feature is disabled
        if (hitFlashEnabled)
            _hitFlashIntensity = MathF.Max(0f, _hitFlashIntensity - HitFlashDecay * frameTime);
        else
            _hitFlashIntensity = 0f;

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
        int ammoCount)
    {
        // Gehenna-Edit: read dynamic settings
        var opacity = Math.Clamp(_cfg.GetCVar(CCVars.CrosshairOpacity), 0f, 1f);
        var outlineEnabled = _cfg.GetCVar(CCVars.CrosshairOutline);
        var centerDotEnabled = _cfg.GetCVar(CCVars.CrosshairCenterDot);
        var baseLineWidth = _cfg.GetCVar(CCVars.CrosshairLineWidth);
        var baseGap = _cfg.GetCVar(CCVars.CrosshairGap);
        var outlineThickness = _cfg.GetCVar(CCVars.CrosshairOutlineThickness);
        var centerDotRadius = _cfg.GetCVar(CCVars.CrosshairCenterDotRadius);

        // Gehenna edit start - firing bloom should pulse visibly on sustained fire
        var impactScale = 1f + _firingImpact * 0.55f;
        gap *= impactScale;
        length *= 1f + _firingImpact * 0.3f;
        // Gehenna edit end
        gap = Math.Clamp(gap, baseGap * uiScale, MaxCrosshairRadius * uiScale);

        bloom = Math.Clamp(bloom, 0f, 1f);
        var shadow = Color.Black.WithAlpha(Math.Clamp(0.84f * opacity, 0f, 1f));
        var shadowOffset = new Vector2(1f, 1f) * uiScale;
        var lineWidth = MathHelper.Lerp(baseLineWidth, baseLineWidth * 0.85f, bloom) * uiScale;
        var outlineWidth = lineWidth + outlineThickness * uiScale;

        if (ammoCount <= 0)
        {
            var blink = 0.45f + MathF.Sin(_drawTime * 9f) * 0.2f;
            var emptyColor = Color.Red.WithAlpha(Math.Clamp(blink * opacity, 0f, 1f));
            var emptyShadow = Color.Black.WithAlpha(Math.Clamp(0.65f * opacity, 0f, 1f));
            var size = Math.Clamp((gap + length * 0.7f) * 0.75f, 9f * uiScale, 22f * uiScale);

            if (outlineEnabled)
            {
                DrawThickLine(screen, center + shadowOffset + new Vector2(-size, -size), center + shadowOffset + new Vector2(size, size), emptyShadow, outlineWidth);
                DrawThickLine(screen, center + shadowOffset + new Vector2(size, -size), center + shadowOffset + new Vector2(-size, size), emptyShadow, outlineWidth);
            }
            DrawThickLine(screen, center + new Vector2(-size, -size), center + new Vector2(size, size), emptyColor, lineWidth);
            DrawThickLine(screen, center + new Vector2(size, -size), center + new Vector2(-size, size), emptyColor, lineWidth);
            if (centerDotEnabled)
            {
                if (outlineEnabled)
                    screen.DrawCircle(center, (centerDotRadius + CenterDotOutlineBonus) * uiScale, emptyShadow, true);
                screen.DrawCircle(center, centerDotRadius * uiScale, emptyColor, true);
            }
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
