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

        var gunUid = GetEntity(ev.Gun);
        if (!TryComp<GunComponent>(gunUid, out var gun))
            return;

        TryStartAiming(user, (gunUid, gun));
    }

    private void OnStopAimingRequest(RequestStopAimingEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        var gunUid = GetEntity(ev.Gun);
        if (TryComp<ActiveAimingComponent>(user, out var active) &&
            active.Weapon != null &&
            active.Weapon != gunUid)
        {
            return;
        }

        TryStopAiming(user);
    }

    private void OnActiveAimingShutdown(Entity<ActiveAimingComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.CurrentEyeOffset = default;
        ent.Comp.TargetEyeOffset = default;
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
    }

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
        active.CurrentEyeOffset = default;
        active.TargetEyeOffset = default;
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
