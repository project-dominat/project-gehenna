using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
/// Sent to the shooter's client when their shot dealt damage > 0.
/// Used to trigger hit-flash feedback on the local crosshair.
/// </summary>
[Serializable, NetSerializable]
public sealed class GunHitConfirmedEvent : EntityEventArgs
{
    public NetEntity Shooter;
    public NetEntity Target;

    public GunHitConfirmedEvent(NetEntity shooter, NetEntity target)
    {
        Shooter = shooter;
        Target = target;
    }
}
