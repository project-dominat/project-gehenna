using Robust.Shared.Serialization;

namespace Content.Shared._Gehenna.Medical.Trauma;

[Serializable, NetSerializable]
public sealed class GehennaSetTargetZoneEvent(NetEntity target, GehennaBodyZone zone) : EntityEventArgs
{
    public readonly NetEntity Target = target;
    public readonly GehennaBodyZone Zone = zone;
}
