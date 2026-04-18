using Content.Shared.Damage;

namespace Content.Shared.Projectiles;

/// <summary>
/// Raised on the projectile entity (server-side) after it successfully dealt damage > 0.
/// </summary>
[ByRefEvent]
public record struct ProjectileDamageAppliedEvent(EntityUid Shooter, EntityUid Target, DamageSpecifier Damage);
