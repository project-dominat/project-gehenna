using Robust.Shared.Serialization;

namespace Content.Shared._Gehenna.Medical.Trauma;

[Serializable, NetSerializable]
public enum GehennaBodyZone : byte
{
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
}

[Serializable, NetSerializable]
public enum GehennaTraumaType : byte
{
    Bruise,
    Cut,
    Puncture,
    Gunshot,
    Burn,
}

[Serializable, NetSerializable]
public enum GehennaWoundState : byte
{
    Open,
    Bandaged,
    Sutured,
    Rotting,
    Septic,
}

[Serializable, NetSerializable]
public enum GehennaTreatmentKind : byte
{
    Bandage,
    Suture,
    Ointment,
}
