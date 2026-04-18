using System.Numerics;
using Content.Client.CombatMode;
using Content.Client.Weapons.Ranged;
using Content.Client.Viewport;
using Content.Shared.Camera;
using Content.Shared.CCVar;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Input;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed class ClientAimingSystem : EntitySystem
{
    [Dependency] private readonly SharedAimingSystem _aiming = default!;
    [Dependency] private readonly CombatModeSystem _combatMode = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly InputSystem _inputSystem = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly ClientWeaponSwaySystem _sway = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private const float EdgeOffset = 0.8f;
    private const float HipFireVisualSpread = 0.11f;
    private const float AimGapFilterSpeed = 18f;
    private const float AimEyeOffsetBaseSmoothTime = 0.12f;
    private const float AimEyeOffsetMinSmoothTime = 0.06f;
    private const float AimEyeOffsetMaxSmoothTime = 0.22f;

    private readonly AimState _aimState = new();
    private readonly AimEyeOffsetState _eyeOffsetState = new();
    private float _adsSmoothTime;

    private ISawmill _sawmill = default!;
    private ICursor? _invisibleCursor;
    private bool _cursorHidden;
    private bool _cursorHideFailed;
    private bool _warnedCursorFail;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("weapons.crosshair");

        Subs.CVar(_cfg, CCVars.WeaponAimAdsSmoothTime, v => _adsSmoothTime = v, true);
        Subs.CVar(_cfg, CCVars.WeaponAimDebugOverlay, OnAimDebugOverlayChanged, true);

        SubscribeLocalEvent<EyeComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnLocalPlayerDetached);
        SubscribeNetworkEvent<GunHitConfirmedEvent>(OnGunHitConfirmed);

        TryInitializeInvisibleCursor();

        _overlay.AddOverlay(new AimingCrosshairOverlay(EntityManager, _eyeManager, _input, _player, _combatMode, _gun, this, _transform, _timing));
    }

    private void OnGunHitConfirmed(GunHitConfirmedEvent ev)
    {
        if (_overlay.TryGetOverlay<AimingCrosshairOverlay>(out var overlay))
            overlay.NotifyHit();
    }

    public override void Shutdown()
    {
        RestoreCursor();
        _overlay.RemoveOverlay<AimingCrosshairOverlay>();
        _overlay.RemoveOverlay<AimingDebugOverlay>();

        base.Shutdown();
    }

    private void TryInitializeInvisibleCursor()
    {
        try
        {
            using var image = new Image<Rgba32>(1, 1);
            image[0, 0] = new Rgba32(0, 0, 0, 0);
            _invisibleCursor = _clyde.CreateCursor(image, Vector2i.Zero);
        }
        catch (System.Exception ex)
        {
            _cursorHideFailed = true;
            _sawmill.Warning($"Failed to create invisible cursor, crosshair will render over system cursor: {ex.Message}");
        }
    }

    private void OnLocalPlayerDetached(LocalPlayerDetachedEvent args)
    {
        RestoreCursor();
    }

    private void OnAimDebugOverlayChanged(bool enabled)
    {
        if (enabled)
        {
            _overlay.AddOverlay(new AimingDebugOverlay(
                EntityManager,
                _player,
                _gun,
                this,
                _resourceCache));
        }
        else
        {
            _overlay.RemoveOverlay<AimingDebugOverlay>();
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        if (_player.LocalEntity is not { } player)
            return;

        var shouldAim = ShouldAim(player, out var gun);

        if (shouldAim)
        {
            if (!TryComp<ActiveAimingComponent>(player, out var active) || active.Weapon != gun.Owner)
            {
                RaisePredictiveEvent(new RequestStartAimingEvent
                {
                    Gun = GetNetEntity(gun.Owner)
                });
            }

            return;
        }

        if (!TryComp<ActiveAimingComponent>(player, out var aiming) || aiming.Weapon is not { } weapon)
            return;

        RaisePredictiveEvent(new RequestStopAimingEvent
        {
            Gun = GetNetEntity(weapon)
        });
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        UpdateAimEyeOffsetState(frameTime);
        UpdateAimState(frameTime);
        UpdateCursorVisibility();
    }

    private void OnGetEyeOffset(Entity<EyeComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (ent.Owner != _player.LocalEntity)
            return;

        if (!_eyeOffsetState.Active && _eyeOffsetState.CurrentOffset.LengthSquared() < 0.0001f)
            return;

        args.Offset += _eyeOffsetState.CurrentOffset;
    }

    public bool TryGetShootCoordinates(
        EntityUid user,
        Entity<GunComponent> gun,
        MapCoordinates mouseCoordinates,
        out MapCoordinates aimCoordinates,
        out MapCoordinates rawAimCoordinates,
        out float visualSpread)
    {
        aimCoordinates = mouseCoordinates;
        rawAimCoordinates = mouseCoordinates;
        visualSpread = HipFireVisualSpread;

        if (!_combatMode.IsInCombatMode(user))
            return true;

        // Gehenna edit start - use the real cursor point; no close-range deadzone magnet
        if (!TryGetRawAimBase(user, mouseCoordinates, out rawAimCoordinates))
            return false;
        // Gehenna edit end

        var input = GetAimInput(user, gun);
        var totalOffset = input.SwayOffset + input.RecoilOffset;
        // Gehenna edit start - farther cursor increases bullet spread
        var distanceSpreadMultiplier = GetAimDistanceSpreadMultiplier(user, rawAimCoordinates);
        totalOffset *= distanceSpreadMultiplier;
        // Gehenna edit end
        // Gehenna edit start - spread only along the lateral aim axis
        var lateralOffset = ProjectAimOffsetToLateralAxis(user, gun, rawAimCoordinates, totalOffset);
        aimCoordinates = new MapCoordinates(rawAimCoordinates.Position + lateralOffset, rawAimCoordinates.MapId);
        // Gehenna edit end
        visualSpread = GetVisualSpread(input) * distanceSpreadMultiplier;

        return true;
    }

    public bool TryGetLocalCrosshair(out Vector2 screenPosition, out Entity<GunComponent> gun, out float visualSpread)
    {
        screenPosition = default;
        gun = default;
        visualSpread = HipFireVisualSpread;

        if (!ShouldDrawLocalCrosshair(out var player, out var currentGun))
            return false;

        gun = currentGun;

        if (!_aimState.Valid ||
            _aimState.Player != player ||
            _aimState.Weapon != currentGun.Owner ||
            _timing.CurTime - _aimState.LastUpdated > TimeSpan.FromSeconds(0.25))
        {
            UpdateAimState(0f);
        }

        if (!_aimState.Valid)
            return false;

        screenPosition = _input.MouseScreenPosition.Position;
        visualSpread = _aimState.VisualSpread;
        return true;
    }

    public bool ShouldDrawLocalCrosshair(out EntityUid player, out Entity<GunComponent> gun)
    {
        player = default;
        gun = default;

        if (_player.LocalEntity is not { } localPlayer ||
            !_combatMode.IsInCombatMode(localPlayer) ||
            !_input.MouseScreenPosition.IsValid ||
            !_gun.TryGetGun(localPlayer, out gun))
        {
            return false;
        }

        if (_uiMgr.KeyboardFocused != null)
            return false;

        var aiming = CompOrNull<AimingComponent>(gun.Owner);
        if (aiming?.ShowCrosshair == false)
            return false;

        player = localPlayer;
        return true;
    }

    internal bool TryGetLocalAimDebugTelemetry(out AimDebugTelemetry telemetry)
    {
        telemetry = default;

        if (_player.LocalEntity is not { } player)
            return false;

        Entity<GunComponent> gun = default;
        var hasGun = _gun.TryGetGun(player, out gun) && HasComp<GunComponent>(gun.Owner);
        var inCombatMode = _combatMode.IsInCombatMode(player);

        EntityUid? weapon = null;
        MapCoordinates? rawTarget = null;
        MapCoordinates? aimedTarget = null;
        Vector2? swayOffset = null;
        Vector2? recoilOffset = null;
        float? estimatedEnvelopeRadius = null;

        if (hasGun)
        {
            weapon = gun.Owner;
            estimatedEnvelopeRadius = EstimateLocalAimEnvelopeRadius(player, gun);

            if (TryGetAimDebugState(player, gun, out var raw, out var aimed, out var sway, out var recoil))
            {
                rawTarget = raw;
                aimedTarget = aimed;
                swayOffset = sway;
                recoilOffset = recoil;
            }
        }

        telemetry = new AimDebugTelemetry(
            player,
            weapon,
            hasGun,
            inCombatMode,
            rawTarget,
            aimedTarget,
            swayOffset,
            recoilOffset,
            estimatedEnvelopeRadius);

        return true;
    }

    public void ApplyShotFeedback(Entity<GunComponent> gun, Vector2 recoilDirection, bool spreadShot)
    {
        _sway.ApplyShotFeedback(gun, recoilDirection, spreadShot);
    }

    private AimInput GetAimInput(EntityUid user, Entity<GunComponent> gun)
    {
        var deliberateAim = false;
        var startedAt = TimeSpan.Zero;
        if (TryComp<ActiveAimingComponent>(user, out var active) &&
            active.Weapon == gun.Owner)
        {
            deliberateAim = true;
            startedAt = active.StartedAt;
        }

        var sway = CompOrNull<WeaponSwayComponent>(gun.Owner);
        var maxSway = deliberateAim
            ? sway?.MaxSway ?? WeaponSwayComponent.DefaultMaxSway
            : sway?.HipFireMaxSway ?? WeaponSwayComponent.DefaultHipFireMaxSway;

        if (_sway.TryGetSwayOffset(
                user,
                gun.Owner,
                deliberateAim,
                startedAt,
                out var swayOffset,
                out var recoilOffset,
                out var amplitude,
                out var movementFactor))
        {
            return new AimInput(swayOffset, recoilOffset, amplitude, movementFactor, maxSway);
        }

        return new AimInput(Vector2.Zero, Vector2.Zero, 0f, 0f, maxSway);
    }

    private void UpdateAimState(float frameTime)
    {
        if (!ShouldDrawLocalCrosshair(out var player, out var gun))
        {
            _aimState.Valid = false;
            return;
        }

        var mouseCoordinates = _eyeManager.PixelToMap(_input.MouseScreenPosition);
        if (mouseCoordinates.MapId == MapId.Nullspace)
        {
            _aimState.Valid = false;
            return;
        }

        var input = GetAimInput(player, gun);
        // Gehenna edit start - debug telemetry uses the same raw cursor target as shooting
        if (!TryGetRawAimBase(player, mouseCoordinates, out var rawAimCoordinates))
        {
            _aimState.Valid = false;
            return;
        }
        // Gehenna edit end

        // Gehenna edit start - farther cursor increases visible and actual spread
        var distanceSpreadMultiplier = GetAimDistanceSpreadMultiplier(player, rawAimCoordinates);
        var visualSpread = GetVisualSpread(input) * distanceSpreadMultiplier;
        // Gehenna edit end
        var shouldReset = !_aimState.Valid ||
                          _aimState.Player != player ||
                          _aimState.Weapon != gun.Owner ||
                          _aimState.MapId != mouseCoordinates.MapId;

        if (shouldReset)
        {
            _aimState.Valid = true;
            _aimState.Player = player;
            _aimState.Weapon = gun.Owner;
            _aimState.MapId = mouseCoordinates.MapId;
            _aimState.VisualSpread = visualSpread;
        }
        else
        {
            var frameAlpha = LowPassAlpha(AimGapFilterSpeed, frameTime);
            _aimState.VisualSpread = MathHelper.Lerp(_aimState.VisualSpread, visualSpread, frameAlpha);
        }

        // Gehenna edit start - debug telemetry mirrors lateral-only shot spread
        _aimState.RawPosition = rawAimCoordinates.Position;
        _aimState.LastSwayOffset = ProjectAimOffsetToLateralAxis(player, gun, rawAimCoordinates, input.SwayOffset * distanceSpreadMultiplier);
        _aimState.LastRecoilOffset = ProjectAimOffsetToLateralAxis(player, gun, rawAimCoordinates, input.RecoilOffset * distanceSpreadMultiplier);
        // Gehenna edit end
        _aimState.LastUpdated = _timing.CurTime;
    }

    private void UpdateCursorVisibility()
    {
        if (_cursorHideFailed)
        {
            if (!_warnedCursorFail)
            {
                _sawmill.Warning("Invisible cursor unavailable; crosshair draws above system cursor.");
                _warnedCursorFail = true;
            }

            return;
        }

        var shouldHide = ShouldDrawLocalCrosshair(out _, out _);

        if (shouldHide)
            HideCursor();
        else
            RestoreCursor();
    }

    private void HideCursor()
    {
        if (_cursorHideFailed || _invisibleCursor == null)
            return;

        if (_cursorHidden)
            return;

        try
        {
            _clyde.SetCursor(_invisibleCursor);
            _cursorHidden = true;
        }
        catch (System.Exception ex)
        {
            _cursorHideFailed = true;
            _sawmill.Warning($"SetCursor failed: {ex.Message}");
        }
    }

    private void RestoreCursor()
    {
        if (!_cursorHidden)
            return;

        try
        {
            _clyde.SetCursor(null);
        }
        catch (System.Exception ex)
        {
            _sawmill.Warning($"SetCursor(null) failed: {ex.Message}");
        }

        _cursorHidden = false;
    }

    private static float GetVisualSpread(AimInput input)
    {
        // Gehenna edit start - crosshair should visibly expose the active recoil/sway envelope
        var movementSpread = input.MovementFactor * 0.14f;
        var envelopeHint = input.MaxSway * 0.25f;
        return Math.Max(
            HipFireVisualSpread,
            Math.Max(input.Amplitude, movementSpread) + input.RecoilOffset.Length() * 0.9f + envelopeHint);
        // Gehenna edit end
    }

    private static float LowPassAlpha(float speed, float frameTime)
    {
        if (frameTime <= 0f)
            return 1f;

        return Math.Clamp(1f - MathF.Exp(-speed * Math.Clamp(frameTime, 0f, 0.1f)), 0f, 1f);
    }

    private static Vector2 SmoothDamp(
        Vector2 current,
        Vector2 target,
        ref Vector2 velocity,
        float smoothTime,
        float frameTime)
    {
        if (frameTime <= 0f)
            return current;

        smoothTime = Math.Max(0.0001f, smoothTime);
        var omega = 2f / smoothTime;
        var x = omega * Math.Clamp(frameTime, 0f, 0.1f);
        var exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
        var change = current - target;
        var temp = (velocity + omega * change) * frameTime;

        velocity = (velocity - omega * temp) * exp;
        return target + (change + temp) * exp;
    }

    // Gehenna edit start - raw aim point without close-range deadzone magnet
    private bool TryGetRawAimBase(
        EntityUid user,
        MapCoordinates mouseCoordinates,
        out MapCoordinates rawAimCoordinates)
    {
        rawAimCoordinates = mouseCoordinates;

        var userCoordinates = _transform.GetMapCoordinates(user);
        if (userCoordinates.MapId == MapId.Nullspace ||
            mouseCoordinates.MapId == MapId.Nullspace ||
            userCoordinates.MapId != mouseCoordinates.MapId)
        {
            return false;
        }

        rawAimCoordinates = mouseCoordinates;
        return true;
    }

    private Vector2 GetFallbackAimDirection(EntityUid user, Entity<GunComponent> gun)
    {
        var direction = gun.Comp.DefaultDirection;
        if (!IsFinite(direction) || direction.LengthSquared() < GunAimValidation.AimAxisEpsilon)
            direction = Vector2.UnitX;

        direction = direction.Normalized();
        var rotated = _transform.GetWorldRotation(user).RotateVec(direction);
        if (!IsFinite(rotated) || rotated.LengthSquared() < GunAimValidation.AimAxisEpsilon)
            return Vector2.UnitX;

        return rotated.Normalized();
    }

    // Gehenna edit start - distance-scaled bullet spread
    private float GetAimDistanceSpreadMultiplier(EntityUid user, MapCoordinates rawAimCoordinates)
    {
        var userCoordinates = _transform.GetMapCoordinates(user);
        if (userCoordinates.MapId == MapId.Nullspace ||
            rawAimCoordinates.MapId == MapId.Nullspace ||
            userCoordinates.MapId != rawAimCoordinates.MapId)
        {
            return 1f;
        }

        return GunAimValidation.ComputeAimDistanceSpreadMultiplier(
            userCoordinates.Position,
            rawAimCoordinates.Position);
    }
    // Gehenna edit end

    // Gehenna edit start - lateral-only aim envelope
    private Vector2 ProjectAimOffsetToLateralAxis(
        EntityUid user,
        Entity<GunComponent> gun,
        MapCoordinates rawAimCoordinates,
        Vector2 offset)
    {
        if (offset == Vector2.Zero)
            return Vector2.Zero;

        var userCoordinates = _transform.GetMapCoordinates(user);
        if (userCoordinates.MapId == MapId.Nullspace ||
            rawAimCoordinates.MapId == MapId.Nullspace ||
            userCoordinates.MapId != rawAimCoordinates.MapId)
        {
            return Vector2.Zero;
        }

        var fallbackDirection = GetFallbackAimDirection(user, gun);
        return GunAimValidation.TryGetAimAxes(
                userCoordinates.Position,
                rawAimCoordinates.Position,
                fallbackDirection,
                out _,
                out var lateralAxis)
            ? GunAimValidation.ProjectOffsetToLateralAxis(offset, lateralAxis)
            : Vector2.Zero;
    }
    // Gehenna edit end

    private static bool IsFinite(Vector2 vector)
    {
        return float.IsFinite(vector.X) && float.IsFinite(vector.Y);
    }
    // Gehenna edit end

    private bool ShouldAim(EntityUid player, out Entity<GunComponent> gun)
    {
        gun = default;

        if (!_combatMode.IsInCombatMode(player) ||
            _inputSystem.CmdStates.GetState(EngineKeyFunctions.UseSecondary) != BoundKeyState.Down ||
            !_input.MouseScreenPosition.IsValid ||
            !_gun.TryGetGun(player, out gun))
        {
            return false;
        }

        return _aiming.CanStartAiming(player, gun);
    }

    private bool TryGetAimDebugState(
        EntityUid user,
        Entity<GunComponent> gun,
        out MapCoordinates rawTarget,
        out MapCoordinates aimedTarget,
        out Vector2 swayOffset,
        out Vector2 recoilOffset)
    {
        rawTarget = default;
        aimedTarget = default;
        swayOffset = default;
        recoilOffset = default;

        if (!_aimState.Valid ||
            _aimState.Player != user ||
            _aimState.Weapon != gun.Owner ||
            _timing.CurTime - _aimState.LastUpdated > TimeSpan.FromSeconds(0.25))
        {
            return false;
        }

        rawTarget = new MapCoordinates(_aimState.RawPosition, _aimState.MapId);
        aimedTarget = new MapCoordinates(_aimState.RawPosition + _aimState.LastSwayOffset + _aimState.LastRecoilOffset, _aimState.MapId);
        swayOffset = _aimState.LastSwayOffset;
        recoilOffset = _aimState.LastRecoilOffset;
        return true;
    }

    private float EstimateLocalAimEnvelopeRadius(EntityUid user, Entity<GunComponent> gun)
    {
        var sway = CompOrNull<WeaponSwayComponent>(gun.Owner);
        var recoil = CompOrNull<GunRecoilComponent>(gun.Owner);
        var isAiming = TryComp<ActiveAimingComponent>(user, out var active) &&
                       active.Weapon == gun.Owner;

        var maxSway = isAiming
            ? sway?.MaxSway ?? WeaponSwayComponent.DefaultMaxSway
            : sway?.HipFireMaxSway ?? WeaponSwayComponent.DefaultHipFireMaxSway;

        var velocityForMax = sway?.VelocityForMaxSway ?? WeaponSwayComponent.DefaultVelocityForMaxSway;
        var velocity = _sway.GetMovementFactor(user, sway) * velocityForMax;
        var movementPenalty = 0f;

        if (velocityForMax > 0f)
        {
            var sprintMultiplier = sway?.SprintSwayMultiplier ?? WeaponSwayComponent.DefaultSprintSwayMultiplier;
            var aimingMovementPenaltyMultiplier = sway?.AimingMovementPenaltyMultiplier ??
                                                  WeaponSwayComponent.DefaultAimingMovementPenaltyMultiplier;

            movementPenalty = GunAimValidation.ComputeMovementPenalty(
                maxSway,
                velocity,
                velocityForMax,
                sprintMultiplier,
                isAiming,
                aimingMovementPenaltyMultiplier);
        }

        var bloom = recoil?.CurrentSwayPenalty ?? 0f;
        var recoilEstimate = recoil?.CurrentRecoilOffset.Length() ?? 0f;

        if (_aimState.Valid &&
            _aimState.Player == user &&
            _aimState.Weapon == gun.Owner)
        {
            recoilEstimate = MathF.Max(recoilEstimate, _aimState.LastRecoilOffset.Length());
        }

        var envelopeRadius = GunAimValidation.ComputeMaxAllowedDeviation(
            maxSway,
            movementPenalty,
            bloom,
            recoilEstimate);

        if (_aimState.Valid &&
            _aimState.Player == user &&
            _aimState.Weapon == gun.Owner &&
            _aimState.MapId != MapId.Nullspace)
        {
            envelopeRadius *= GetAimDistanceSpreadMultiplier(user, new MapCoordinates(_aimState.RawPosition, _aimState.MapId));
            // Gehenna edit start - crosshair envelope also covers projectile pellet spread
            envelopeRadius += EstimateLocalProjectileSpreadRadius(user, gun);
            // Gehenna edit end
        }

        return envelopeRadius;
    }

    // Gehenna edit start - include ammo spread in the visible lateral envelope
    private float EstimateLocalProjectileSpreadRadius(EntityUid user, Entity<GunComponent> gun)
    {
        if (!_aimState.Valid ||
            _aimState.Player != user ||
            _aimState.Weapon != gun.Owner ||
            _aimState.MapId == MapId.Nullspace)
        {
            return 0f;
        }

        if (_gun.GetAmmoCount(gun.Owner) <= 0)
            return 0f;

        var userCoordinates = _transform.GetMapCoordinates(user);
        if (userCoordinates.MapId == MapId.Nullspace ||
            userCoordinates.MapId != _aimState.MapId)
        {
            return 0f;
        }

        if (!TryGetAmmoProviderProjectileSpread(gun.Owner, out var spread))
            return 0f;

        var spreadEvent = new GunGetAmmoSpreadEvent(spread);
        RaiseLocalEvent(gun.Owner, ref spreadEvent);

        var aimDistance = (_aimState.RawPosition - userCoordinates.Position).Length();
        return GunAimValidation.ComputeLateralProjectileSpreadRadius(aimDistance, spreadEvent.Spread);
    }

    private bool TryGetAmmoProviderProjectileSpread(EntityUid provider, out Angle spread, int depth = 0)
    {
        spread = Angle.Zero;

        if (depth > 4)
            return false;

        if (TryComp<ChamberMagazineAmmoProviderComponent>(provider, out var chamber) &&
            chamber.BoltClosed != false &&
            TryGetContainedEntity(provider, SharedGunSystem.ChamberSlot, out var chambered))
        {
            return TryGetAmmoEntityProjectileSpread(chambered, out spread);
        }

        if (TryComp<MagazineAmmoProviderComponent>(provider, out _) &&
            TryGetContainedEntity(provider, SharedGunSystem.MagazineSlot, out var magazine))
        {
            return TryGetAmmoProviderProjectileSpread(magazine, out spread, depth + 1);
        }

        if (TryComp<BallisticAmmoProviderComponent>(provider, out var ballistic))
            return TryGetBallisticProjectileSpread(ballistic, out spread);

        if (TryComp<BasicEntityAmmoProviderComponent>(provider, out var basic))
            return TryGetAmmoPrototypeProjectileSpread(basic.Proto, out spread);

        if (TryComp<BatteryAmmoProviderComponent>(provider, out var battery))
            return TryGetAmmoPrototypeProjectileSpread(battery.Prototype, out spread);

        if (TryComp<SolutionAmmoProviderComponent>(provider, out var solution))
            return TryGetAmmoPrototypeProjectileSpread(solution.Prototype, out spread);

        if (TryComp<ContainerAmmoProviderComponent>(provider, out var containerProvider))
            return TryGetContainerProjectileSpread((provider, containerProvider), out spread);

        return TryGetAmmoEntityProjectileSpread(provider, out spread);
    }

    private bool TryGetBallisticProjectileSpread(BallisticAmmoProviderComponent ballistic, out Angle spread)
    {
        spread = Angle.Zero;

        if (ballistic.Entities.Count > 0)
            return TryGetAmmoEntityProjectileSpread(ballistic.Entities[^1], out spread);

        if (ballistic.UnspawnedCount <= 0 || ballistic.Proto == null)
            return false;

        return TryGetAmmoPrototypeProjectileSpread(ballistic.Proto.Value, out spread);
    }

    private bool TryGetContainerProjectileSpread(Entity<ContainerAmmoProviderComponent> provider, out Angle spread)
    {
        spread = Angle.Zero;
        var containerOwner = provider.Comp.ProviderUid ?? provider.Owner;

        if (!_containers.TryGetContainer(containerOwner, provider.Comp.Container, out var container) ||
            container.ContainedEntities.Count == 0)
        {
            return false;
        }

        return TryGetAmmoEntityProjectileSpread(container.ContainedEntities[0], out spread);
    }

    private bool TryGetContainedEntity(EntityUid owner, string containerId, out EntityUid contained)
    {
        contained = default;

        if (!_containers.TryGetContainer(owner, containerId, out var container) ||
            container is not ContainerSlot { ContainedEntity: { } entity })
        {
            return false;
        }

        contained = entity;
        return true;
    }

    private bool TryGetAmmoEntityProjectileSpread(EntityUid ammo, out Angle spread)
    {
        spread = Angle.Zero;

        if (TryComp<ProjectileSpreadComponent>(ammo, out var projectileSpread) &&
            projectileSpread.Count > 1)
        {
            spread = projectileSpread.Spread;
            return true;
        }

        if (TryComp<CartridgeAmmoComponent>(ammo, out var cartridge) && !cartridge.Spent)
            return TryGetAmmoPrototypeProjectileSpread(cartridge.Prototype, out spread);

        return false;
    }

    private bool TryGetAmmoPrototypeProjectileSpread(EntProtoId prototype, out Angle spread)
    {
        spread = Angle.Zero;

        if (!_prototype.TryIndex<EntityPrototype>(prototype, out var entityPrototype))
            return false;

        if (entityPrototype.TryGetComponent<ProjectileSpreadComponent>(
                out var projectileSpread,
                EntityManager.ComponentFactory) &&
            projectileSpread.Count > 1)
        {
            spread = projectileSpread.Spread;
            return true;
        }

        return entityPrototype.TryGetComponent<CartridgeAmmoComponent>(
                   out var cartridge,
                   EntityManager.ComponentFactory) &&
               !cartridge.Spent &&
               TryGetAmmoPrototypeProjectileSpread(cartridge.Prototype, out spread);
    }
    // Gehenna edit end

    private void UpdateAimEyeOffsetState(float frameTime)
    {
        if (_player.LocalEntity is not { } player)
        {
            ResetAimEyeOffsetState();
            return;
        }

        var target = Vector2.Zero;
        var smoothTime = AimEyeOffsetBaseSmoothTime;
        var active = ShouldAim(player, out var gun) && TryGetTargetEyeOffset(gun.Owner, out target, out smoothTime);

        if (_eyeOffsetState.Player != player)
        {
            _eyeOffsetState.Player = player;
            _eyeOffsetState.CurrentOffset = Vector2.Zero;
            _eyeOffsetState.Velocity = Vector2.Zero;
        }

        if (active && _eyeOffsetState.Weapon != gun.Owner)
        {
            _eyeOffsetState.Weapon = gun.Owner;
            _eyeOffsetState.Velocity = Vector2.Zero;
        }
        else if (!active)
        {
            _eyeOffsetState.Weapon = null;
        }

        _eyeOffsetState.Active = active;
        _eyeOffsetState.TargetOffset = target;
        _eyeOffsetState.CurrentOffset = SmoothDamp(
            _eyeOffsetState.CurrentOffset,
            target,
            ref _eyeOffsetState.Velocity,
            smoothTime,
            frameTime);

        if (!active && _eyeOffsetState.CurrentOffset.LengthSquared() < 0.0001f)
        {
            _eyeOffsetState.CurrentOffset = Vector2.Zero;
            _eyeOffsetState.TargetOffset = Vector2.Zero;
            _eyeOffsetState.Velocity = Vector2.Zero;
        }
    }

    private void ResetAimEyeOffsetState()
    {
        _eyeOffsetState.Active = false;
        _eyeOffsetState.Player = default;
        _eyeOffsetState.Weapon = null;
        _eyeOffsetState.CurrentOffset = Vector2.Zero;
        _eyeOffsetState.TargetOffset = Vector2.Zero;
        _eyeOffsetState.Velocity = Vector2.Zero;
    }

    private bool TryGetTargetEyeOffset(EntityUid weapon, out Vector2 target, out float smoothTime)
    {
        target = Vector2.Zero;
        smoothTime = AimEyeOffsetBaseSmoothTime;

        if (_eyeManager.MainViewport is not ScalingViewport vp)
            return false;

        var aiming = CompOrNull<AimingComponent>(weapon);
        var maxOffset = aiming?.EyeOffset ?? AimingComponent.DefaultEyeOffset;
        var offsetSpeed = aiming?.EyeOffsetSpeed ?? AimingComponent.DefaultEyeOffsetSpeed;
        smoothTime = !float.IsNaN(_adsSmoothTime) && _adsSmoothTime > 0f
            ? _adsSmoothTime
            : GetAimEyeSmoothTime(offsetSpeed);

        if (maxOffset <= 0f)
            return true;

        var mousePos = _input.MouseScreenPosition.Position;
        var viewportSize = vp.PixelSize;
        var scalingViewportSize = vp.ViewportSize * vp.CurrentRenderScale;
        var visibleViewportSize = Vector2.Min(viewportSize, scalingViewportSize);

        Matrix3x2.Invert(_eyeManager.MainViewport.GetLocalToScreenMatrix(), out var matrix);
        var mouseCoords = Vector2.Transform(mousePos, matrix);
        var boundedMousePos = Vector2.Clamp(Vector2.Min(mouseCoords, mousePos), Vector2.Zero, visibleViewportSize);

        var offsetRadius = MathF.Min(visibleViewportSize.X / 2f, visibleViewportSize.Y / 2f) * EdgeOffset;
        if (offsetRadius <= 0f)
            return true;

        var mouseNormalizedPos = new Vector2(
            -(boundedMousePos.X - visibleViewportSize.X / 2f) / offsetRadius,
            (boundedMousePos.Y - visibleViewportSize.Y / 2f) / offsetRadius);

        if (_input.MouseScreenPosition.Window == WindowId.Invalid)
            return true;

        var eyeRotation = _eyeManager.CurrentEye.Rotation;
        target = Vector2.Transform(
            mouseNormalizedPos,
            Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, (float)eyeRotation.Opposite().Theta));

        target *= maxOffset;
        if (target.Length() > maxOffset)
            target = target.Normalized() * maxOffset;

        return true;
    }

    private static float GetAimEyeSmoothTime(float offsetSpeed)
    {
        if (offsetSpeed <= 0f)
            return AimEyeOffsetMaxSmoothTime;

        var speedScale = AimingComponent.DefaultEyeOffsetSpeed / offsetSpeed;
        return Math.Clamp(
            AimEyeOffsetBaseSmoothTime * speedScale,
            AimEyeOffsetMinSmoothTime,
            AimEyeOffsetMaxSmoothTime);
    }

    private sealed class AimState
    {
        public bool Valid;
        public EntityUid Player;
        public EntityUid Weapon;
        public MapId MapId;
        public Vector2 RawPosition;
        public Vector2 LastSwayOffset;
        public Vector2 LastRecoilOffset;
        public float VisualSpread;
        public TimeSpan LastUpdated;
    }

    private sealed class AimEyeOffsetState
    {
        public bool Active;
        public EntityUid Player;
        public EntityUid? Weapon;
        public Vector2 CurrentOffset;
        public Vector2 TargetOffset;
        public Vector2 Velocity;
    }

    private readonly record struct AimInput(
        Vector2 SwayOffset,
        Vector2 RecoilOffset,
        float Amplitude,
        float MovementFactor,
        float MaxSway);
}

internal readonly record struct AimDebugTelemetry(
    EntityUid Player,
    EntityUid? Weapon,
    bool HasGun,
    bool InCombatMode,
    MapCoordinates? RawTarget,
    MapCoordinates? AimedTarget,
    Vector2? SwayOffset,
    Vector2? RecoilOffset,
    float? EstimatedEnvelopeRadius);
