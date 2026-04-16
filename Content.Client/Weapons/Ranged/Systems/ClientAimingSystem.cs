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
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed class ClientAimingSystem : EntitySystem
{
    [Dependency] private readonly SharedAimingSystem _aiming = default!;
    [Dependency] private readonly CombatModeSystem _combatMode = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [Dependency] private readonly InputSystem _inputSystem = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly ClientWeaponSwaySystem _sway = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    private const float EdgeOffset = 0.8f;
    private const float HipFireVisualSpread = 0.11f;

    private ICursor? _blankCursor;
    private ICursor? _previousWorldCursor;
    private ICursor? _previousViewportCursor;
    private ICursor? _previousRootCursor;
    private ICursor? _previousWindowRootCursor;
    private bool _cursorHidden;

    public override void Initialize()
    {
        base.Initialize();

        using var blankImage = new Image<Rgba32>(1, 1);
        _blankCursor = _clyde.CreateCursor(blankImage, Vector2i.Zero);

        SubscribeLocalEvent<ActiveAimingComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
        _overlay.AddOverlay(new AimingCrosshairOverlay(EntityManager, _eyeManager, _player, _combatMode, _gun, this));
    }

    public override void Shutdown()
    {
        RestoreSystemCursor();
        _blankCursor?.Dispose();
        _blankCursor = null;

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

        UpdateCursorVisibility();
    }

    private void OnGetEyeOffset(Entity<ActiveAimingComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (ent.Owner != _player.LocalEntity)
            return;

        var offset = UpdateEyeOffset(ent);
        if (offset == null)
            return;

        args.Offset += offset.Value;
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

        var deliberateAim = false;
        var startedAt = TimeSpan.Zero;
        if (TryComp<ActiveAimingComponent>(user, out var active) &&
            active.Weapon == gun.Owner)
        {
            deliberateAim = true;
            startedAt = active.StartedAt;
        }

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
            aimCoordinates = new MapCoordinates(mouseCoordinates.Position + swayOffset + recoilOffset, mouseCoordinates.MapId);
            visualSpread = Math.Max(HipFireVisualSpread, Math.Max(amplitude, movementFactor * 0.08f) + recoilOffset.Length() * 0.5f);
        }

        return true;
    }

    public bool TryGetLocalCrosshair(out MapCoordinates aimCoordinates, out Entity<GunComponent> gun, out float visualSpread)
    {
        aimCoordinates = default;
        gun = default;
        visualSpread = 0f;

        if (!ShouldDrawLocalCrosshair(out var player, out gun))
        {
            return false;
        }

        var mouseCoordinates = _eyeManager.PixelToMap(_input.MouseScreenPosition);
        if (mouseCoordinates.MapId == MapId.Nullspace)
            return false;

        return TryGetShootCoordinates(player, gun, mouseCoordinates, out aimCoordinates, out visualSpread);
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

    private void UpdateCursorVisibility()
    {
        var shouldHide = ShouldHideSystemCursor();

        if (shouldHide)
            HideSystemCursor();
        else
            RestoreSystemCursor();
    }

    private bool ShouldHideSystemCursor()
    {
        return _player.LocalEntity is { } localPlayer &&
               (_combatMode.IsInCombatMode(localPlayer) || HasComp<ActiveAimingComponent>(localPlayer));
    }

    private void HideSystemCursor()
    {
        if (_blankCursor == null)
            return;

        if (!_cursorHidden)
        {
            _previousWorldCursor = _ui.WorldCursor;
            _previousViewportCursor = _ui.MainViewport.CustomCursorShape;
            _previousRootCursor = _ui.RootControl.CustomCursorShape;
            _previousWindowRootCursor = _ui.WindowRoot.CustomCursorShape;
            _cursorHidden = true;
        }

        _ui.WorldCursor = _blankCursor;
        _ui.MainViewport.CustomCursorShape = _blankCursor;
        _ui.RootControl.CustomCursorShape = _blankCursor;
        _ui.WindowRoot.CustomCursorShape = _blankCursor;
        _clyde.SetCursor(_blankCursor);
    }

    private void RestoreSystemCursor()
    {
        if (!_cursorHidden)
            return;

        _ui.WorldCursor = _previousWorldCursor;
        _ui.MainViewport.CustomCursorShape = _previousViewportCursor;
        _ui.RootControl.CustomCursorShape = _previousRootCursor;
        _ui.WindowRoot.CustomCursorShape = _previousWindowRootCursor;
        _clyde.SetCursor(_previousWorldCursor);
        _previousWorldCursor = null;
        _previousViewportCursor = null;
        _previousRootCursor = null;
        _previousWindowRootCursor = null;
        _cursorHidden = false;
    }

    private Vector2? UpdateEyeOffset(Entity<ActiveAimingComponent> ent)
    {
        if (ent.Comp.Weapon is not { } weapon)
            return null;

        if (_eyeManager.MainViewport is not ScalingViewport vp)
            return null;

        var aiming = CompOrNull<AimingComponent>(weapon);
        var maxOffset = aiming?.EyeOffset ?? AimingComponent.DefaultEyeOffset;
        var offsetSpeed = aiming?.EyeOffsetSpeed ?? AimingComponent.DefaultEyeOffsetSpeed;

        if (maxOffset <= 0f)
        {
            ent.Comp.CurrentEyeOffset = Vector2.Zero;
            ent.Comp.TargetEyeOffset = Vector2.Zero;
            return Vector2.Zero;
        }

        var mousePos = _input.MouseScreenPosition.Position;
        var viewportSize = vp.PixelSize;
        var scalingViewportSize = vp.ViewportSize * vp.CurrentRenderScale;
        var visibleViewportSize = Vector2.Min(viewportSize, scalingViewportSize);

        Matrix3x2.Invert(_eyeManager.MainViewport.GetLocalToScreenMatrix(), out var matrix);
        var mouseCoords = Vector2.Transform(mousePos, matrix);
        var boundedMousePos = Vector2.Clamp(Vector2.Min(mouseCoords, mousePos), Vector2.Zero, visibleViewportSize);

        var offsetRadius = MathF.Min(visibleViewportSize.X / 2f, visibleViewportSize.Y / 2f) * EdgeOffset;
        if (offsetRadius <= 0f)
            return ent.Comp.CurrentEyeOffset;

        var mouseNormalizedPos = new Vector2(
            -(boundedMousePos.X - visibleViewportSize.X / 2f) / offsetRadius,
            (boundedMousePos.Y - visibleViewportSize.Y / 2f) / offsetRadius);

        if (_input.MouseScreenPosition.Window != WindowId.Invalid)
        {
            var eyeRotation = _eyeManager.CurrentEye.Rotation;
            var mouseActualRelativePos = Vector2.Transform(
                mouseNormalizedPos,
                Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, (float) eyeRotation.Opposite().Theta));

            mouseActualRelativePos *= maxOffset;
            if (mouseActualRelativePos.Length() > maxOffset)
                mouseActualRelativePos = mouseActualRelativePos.Normalized() * maxOffset;

            ent.Comp.TargetEyeOffset = mouseActualRelativePos;

            var delta = ent.Comp.TargetEyeOffset - ent.Comp.CurrentEyeOffset;
            if (delta.Length() > offsetSpeed)
                delta = delta.Normalized() * offsetSpeed;

            ent.Comp.CurrentEyeOffset += delta;
        }

        return ent.Comp.CurrentEyeOffset;
    }
}
