using Content.Shared.Projectiles;
using Content.Shared.Weapons.Hitscan.Components;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Player;

namespace Content.Server.Weapons.Ranged.Systems;

/// <summary>
/// Sends <see cref="GunHitConfirmedEvent"/> to the shooter's client whenever their shot deals damage > 0.
/// Covers both hitscan and projectile weapons.
/// Known limitation: projectile riko (PreventCollision reassigns Shooter to the deflector entity)
/// will flash the deflector rather than the original shooter. Accepted edge-case.
/// </summary>
public sealed class GunHitFlashSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProjectileComponent, ProjectileDamageAppliedEvent>(OnProjectileHit);
        SubscribeLocalEvent<HitscanBasicDamageComponent, HitscanDamageDealtEvent>(OnHitscanHit);
    }

    private void OnProjectileHit(Entity<ProjectileComponent> projectile, ref ProjectileDamageAppliedEvent args)
    {
        NotifyShooter(args.Shooter, args.Target);
    }

    private void OnHitscanHit(Entity<HitscanBasicDamageComponent> hitscan, ref HitscanDamageDealtEvent args)
    {
        if (args.Shooter is not { } shooter)
            return;

        NotifyShooter(shooter, args.Target);
    }

    private void NotifyShooter(EntityUid shooter, EntityUid target)
    {
        if (!TryComp<ActorComponent>(shooter, out var actor))
            return;

        var ev = new GunHitConfirmedEvent(GetNetEntity(shooter), GetNetEntity(target));
        RaiseNetworkEvent(ev, Filter.SinglePlayer(actor.PlayerSession));
    }
}
