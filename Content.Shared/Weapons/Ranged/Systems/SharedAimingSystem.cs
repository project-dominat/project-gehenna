using Content.Shared.Camera;
using Content.Shared.CombatMode;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Timing;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class SharedAimingSystem : EntitySystem
{
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly SharedContentEyeSystem _eye = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    /// <summary>
    /// Minimum time between ADS toggle events to prevent spam-toggling for recoil reset.
    /// </summary>
    private static readonly TimeSpan AdsToggleCooldown = TimeSpan.FromSeconds(0.3);

    /// <summary>
    /// Tracks the last time each user toggled ADS, for cooldown enforcement.
    /// </summary>
    private readonly Dictionary<EntityUid, TimeSpan> _lastAdsToggle = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<RequestStartAimingEvent>(OnStartAimingRequest);
        SubscribeAllEvent<RequestStopAimingEvent>(OnStopAimingRequest);

        SubscribeLocalEvent<ActiveAimingComponent, ComponentShutdown>(OnActiveAimingShutdown);
        SubscribeLocalEvent<ActiveAimingComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<ActiveAimingComponent, GetEyePvsScaleEvent>(OnGetEyePvsScale);
    }

    private void OnStartAimingRequest(RequestStartAimingEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        // Gehenna edit start — ADS toggle cooldown
        if (!CheckAdsToggleCooldown(user))
            return;
        // Gehenna edit end

        var gunUid = GetEntity(ev.Gun);
        if (!TryComp<GunComponent>(gunUid, out var gun))
            return;

        TryStartAiming(user, (gunUid, gun));
    }

    private void OnStopAimingRequest(RequestStopAimingEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        // Gehenna edit start — ADS toggle cooldown
        if (!CheckAdsToggleCooldown(user))
            return;
        // Gehenna edit end

        var gunUid = GetEntity(ev.Gun);
        if (TryComp<ActiveAimingComponent>(user, out var active) &&
            active.Weapon != null &&
            active.Weapon != gunUid)
        {
            return;
        }

        TryStopAiming(user);
    }

    /// <summary>
    /// Checks and enforces ADS toggle cooldown. Returns true if the toggle is allowed.
    /// </summary>
    private bool CheckAdsToggleCooldown(EntityUid user)
    {
        // Prediction replays must not mutate cooldown state or ADS reconciliation diverges.
        if (_timing.InPrediction)
            return true;

        var now = _timing.CurTime;

        if (_lastAdsToggle.TryGetValue(user, out var lastToggle) &&
            now - lastToggle < AdsToggleCooldown)
        {
            return false;
        }

        _lastAdsToggle[user] = now;
        return true;
    }

    private void OnActiveAimingShutdown(Entity<ActiveAimingComponent> ent, ref ComponentShutdown args)
    {
        RefreshAimingEffects(ent.Owner);
    }

    private void OnRefreshMovementSpeed(Entity<ActiveAimingComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Weapon is not { } weapon || Deleted(weapon))
            return;

        var aiming = CompOrNull<AimingComponent>(weapon);
        args.ModifySpeed(
            aiming?.WalkModifier ?? AimingComponent.DefaultWalkModifier,
            aiming?.SprintModifier ?? AimingComponent.DefaultSprintModifier);
    }

    private void OnGetEyePvsScale(Entity<ActiveAimingComponent> ent, ref GetEyePvsScaleEvent args)
    {
        if (ent.Comp.Weapon is not { } weapon || Deleted(weapon))
            return;

        var aiming = CompOrNull<AimingComponent>(weapon);
        args.Scale += aiming?.PvsIncrease ?? AimingComponent.DefaultPvsIncrease;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActiveAimingComponent>();
        while (query.MoveNext(out var uid, out var active))
        {
            if (active.Weapon is not { } weapon ||
                Deleted(weapon) ||
                !TryComp<GunComponent>(weapon, out var gun) ||
                !CanContinueAiming(uid, (weapon, gun), active))
            {
                TryStopAiming(uid, active);
            }
        }

        // Gehenna edit start — clean up stale cooldown entries
        if (_lastAdsToggle.Count > 0)
        {
            var expiry = _timing.CurTime - AdsToggleCooldown - AdsToggleCooldown; // 2x cooldown = safe to purge
            _cleanupBuffer.Clear();
            foreach (var (uid, time) in _lastAdsToggle)
            {
                if (Deleted(uid) || time < expiry)
                    _cleanupBuffer.Add(uid);
            }
            foreach (var uid in _cleanupBuffer)
            {
                _lastAdsToggle.Remove(uid);
            }
        }
        // Gehenna edit end
    }

    private readonly List<EntityUid> _cleanupBuffer = [];

    public bool CanStartAiming(EntityUid user, Entity<GunComponent> gun, bool checkCombatMode = true)
    {
        if (Deleted(user) || Deleted(gun.Owner))
            return false;

        if (!_gun.TryGetGun(user, out var heldGun) || heldGun.Owner != gun.Owner)
            return false;

        var aiming = CompOrNull<AimingComponent>(gun.Owner);
        if (checkCombatMode &&
            (aiming?.RequireCombatMode ?? true) &&
            !_combatMode.IsInCombatMode(user))
        {
            return false;
        }

        return true;
    }

    public bool TryStartAiming(EntityUid user, Entity<GunComponent> gun, bool checkCombatMode = true)
    {
        if (!CanStartAiming(user, gun, checkCombatMode))
            return false;

        if (TryComp<ActiveAimingComponent>(user, out var active))
        {
            if (active.Weapon == gun.Owner)
                return true;

            TryStopAiming(user, active);
        }

        active = EnsureComp<ActiveAimingComponent>(user);
        active.Weapon = gun.Owner;
        active.StartedAt = _timing.CurTime;
        Dirty(user, active);

        RefreshAimingEffects(user);
        return true;
    }

    public bool TryStopAiming(EntityUid user, ActiveAimingComponent? active = null)
    {
        if (!Resolve(user, ref active, false))
            return false;

        RemComp<ActiveAimingComponent>(user);
        RefreshAimingEffects(user);
        return true;
    }

    public bool IsAiming(EntityUid user, ActiveAimingComponent? active = null)
    {
        return Resolve(user, ref active, false) &&
               active.Weapon != null &&
               !Deleted(active.Weapon.Value);
    }

    public bool TryGetAimingGun(EntityUid user, out Entity<GunComponent> gun, ActiveAimingComponent? active = null)
    {
        gun = default;

        if (!Resolve(user, ref active, false) ||
            active.Weapon is not { } weapon ||
            !TryComp<GunComponent>(weapon, out var gunComp))
        {
            return false;
        }

        gun = (weapon, gunComp);
        return true;
    }

    private bool CanContinueAiming(EntityUid user, Entity<GunComponent> gun, ActiveAimingComponent active)
    {
        var aiming = CompOrNull<AimingComponent>(gun.Owner);
        return CanStartAiming(user, gun, aiming?.RequireCombatMode ?? true);
    }

    private void RefreshAimingEffects(EntityUid user)
    {
        _movement.RefreshMovementSpeedModifiers(user);
        _eye.UpdatePvsScale(user);
    }
}
