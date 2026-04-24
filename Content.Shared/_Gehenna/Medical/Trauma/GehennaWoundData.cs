using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Gehenna.Medical.Trauma;

[Serializable, NetSerializable]
public sealed class GehennaWoundData
{
    public GehennaBodyZone Zone;
    public GehennaTraumaType Type;
    public GehennaWoundState State = GehennaWoundState.Open;
    public FixedPoint2 Severity;
    public int BurnDegree;
    public TimeSpan CreatedAt;
    public TimeSpan LastTreatedAt;
    public DamageSpecifier Damage = new();
    public FixedPoint2 BleedRate;
    public bool Bleeding;
    public bool Tourniqueted;
}

[Serializable, NetSerializable]
public sealed class GehennaTraumaScannerEntry
{
    public GehennaBodyZone Zone;
    public GehennaTraumaType Type;
    public GehennaWoundState State;
    public FixedPoint2 Severity;
    public int BurnDegree;
    public bool Bleeding;
    public bool Tourniqueted;
    public string Treatment = string.Empty;
}
