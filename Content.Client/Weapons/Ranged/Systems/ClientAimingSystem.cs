using System.Numerics;
using Content.Client.CombatMode;
using Content.Client.Weapons.Ranged;
using Content.Client.Viewport;
using Content.Shared.Camera;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

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
    [Dependency] private readonly ClientWeaponSwaySystem _sway = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    private const float EdgeOffset = 0.8f;
    private const float HipFireVisualSpread = 0.11f;
    private const float AimOffsetFilterSpeed = 18f;
    private const float AimSpringSmoothTime = 0.045f;
    private const float AimSpringMaxLag = 0.22f;
    private const float AimSpringMinLag = 0.05f;
    private const float AimEyeOffsetBaseSmoothTime = 0.12f;
    private const float AimEyeOffsetMinSmoothTime = 0.06f;
    private const float AimEyeOffsetMaxSmoothTime = 0.22f;

    private readonly AimState _aimState = new();
    private readonly AimEyeOffsetState _eyeOffsetState = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EyeComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
        _overlay.AddOverlay(new AimingCrosshairOverlay(EntityManager, _eyeManager, _player, _combatMode, _gun, this, _timing));
    }

    public override void Shutdown()
    {
        _overlay.RemoveOverlay<AimingCrosshairOverlay>();

        base.Shutdown();
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
        out float visualSpread)
    {
        aimCoordinates = mouseCoordinates;
        visualSpread = HipFireVisualSpread;

        if (!_combatMode.IsInCombatMode(user))
            return true;

        if (TryUseSmoothedAimState(user, gun, mouseCoordinates.MapId, out aimCoordinates, out visualSpread))
            return true;

        var input = GetAimInput(user, gun);
        var totalOffset = input.SwayOffset + input.RecoilOffset;
        aimCoordinates = new MapCoordinates(mouseCoordinates.Position + totalOffset, mouseCoordinates.MapId);
        visualSpread = GetVisualSpread(input);

        return true;
    }

    public bool TryGetLocalCrosshair(out MapCoordinates aimCoordinates, out Entity<GunComponent> gun, out float visualSpread)
    {
        aimCoordinates = default;
        gun = default;
        visualSpread = 0f;

        if (!ShouldDrawLocalCrosshair(out var player, out var currentGun))
            return false;

        gun = currentGun;

        if (!TryUseSmoothedAimState(player, currentGun, _aimState.MapId, out aimCoordinates, out visualSpread))
        {
            UpdateAimState(0f);
            if (!TryUseSmoothedAimState(player, currentGun, _aimState.MapId, out aimCoordinates, out visualSpread))
                return false;
        }

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

        var aiming = CompOrNull<AimingComponent>(gun.Owner);
        if (aiming?.ShowCrosshair == false)
            return false;

        player = localPlayer;
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
        var frameAlpha = LowPassAlpha(AimOffsetFilterSpeed, frameTime);
        var visualSpread = GetVisualSpread(input);
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
            _aimState.FilteredSwayOffset = input.SwayOffset;
            _aimState.FilteredRecoilOffset = input.RecoilOffset;
            _aimState.VisualSpread = visualSpread;
            _aimState.Position = mouseCoordinates.Position + input.SwayOffset + input.RecoilOffset;
            _aimState.Velocity = Vector2.Zero;
        }
        else
        {
            _aimState.FilteredSwayOffset = Vector2.Lerp(_aimState.FilteredSwayOffset, input.SwayOffset, frameAlpha);
            _aimState.FilteredRecoilOffset = Vector2.Lerp(_aimState.FilteredRecoilOffset, input.RecoilOffset, frameAlpha);
            _aimState.VisualSpread = MathHelper.Lerp(_aimState.VisualSpread, visualSpread, frameAlpha);
        }

        var totalOffset = _aimState.FilteredSwayOffset + _aimState.FilteredRecoilOffset;
        var targetPosition = mouseCoordinates.Position + totalOffset;

        if (!shouldReset)
        {
            _aimState.Position = SmoothDamp(
                _aimState.Position,
                targetPosition,
                ref _aimState.Velocity,
                AimSpringSmoothTime,
                frameTime);
        }

        var maxLag = Math.Clamp(input.MaxSway * 0.35f, AimSpringMinLag, AimSpringMaxLag);
        var lag = targetPosition - _aimState.Position;
        if (lag.Length() > maxLag)
        {
            _aimState.Position = targetPosition - lag.Normalized() * maxLag;
            _aimState.Velocity = Vector2.Zero;
        }

        _aimState.TargetPosition = targetPosition;
        _aimState.TotalOffset = totalOffset;
        _aimState.LastUpdated = _timing.CurTime;
    }

    private bool TryUseSmoothedAimState(
        EntityUid user,
        Entity<GunComponent> gun,
        MapId mapId,
        out MapCoordinates aimCoordinates,
        out float visualSpread)
    {
        aimCoordinates = default;
        visualSpread = HipFireVisualSpread;

        if (!_aimState.Valid ||
            _aimState.Player != user ||
            _aimState.Weapon != gun.Owner ||
            _aimState.MapId != mapId ||
            _timing.CurTime - _aimState.LastUpdated > TimeSpan.FromSeconds(0.25))
        {
            return false;
        }

        aimCoordinates = new MapCoordinates(_aimState.Position, _aimState.MapId);
        visualSpread = _aimState.VisualSpread;
        return true;
    }

    private static float GetVisualSpread(AimInput input)
    {
        return Math.Max(HipFireVisualSpread, Math.Max(input.Amplitude, input.MovementFactor * 0.08f) + input.RecoilOffset.Length() * 0.5f);
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
        smoothTime = GetAimEyeSmoothTime(offsetSpeed);

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
        public Vector2 FilteredSwayOffset;
        public Vector2 FilteredRecoilOffset;
        public Vector2 TotalOffset;
        public Vector2 TargetPosition;
        public Vector2 Position;
        public Vector2 Velocity;
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
