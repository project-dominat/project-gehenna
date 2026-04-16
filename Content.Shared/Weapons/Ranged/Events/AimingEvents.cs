using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
/// Client request to start deliberately aiming a gun.
/// </summary>
[Serializable, NetSerializable]
public sealed class RequestStartAimingEvent : EntityEventArgs
{
    public NetEntity Gun;
}

/// <summary>
/// Client request to stop deliberately aiming a gun.
/// </summary>
[Serializable, NetSerializable]
public sealed class RequestStopAimingEvent : EntityEventArgs
{
    public NetEntity Gun;
}
