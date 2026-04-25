using Content.Shared.MedicalScanner;
using Content.Shared.Mobs;
using Robust.Shared.Serialization;

namespace Content.Shared._Gehenna.Medical.Trauma;

[Serializable, NetSerializable]
public enum GehennaTraumaScannerUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class GehennaTraumaScannerScannedUserMessage : BoundUserInterfaceMessage
{
    public GehennaTraumaScannerUiState State;

    public GehennaTraumaScannerScannedUserMessage(GehennaTraumaScannerUiState state)
    {
        State = state;
    }
}

[Serializable, NetSerializable]
public sealed class GehennaTraumaScannerUiState : Robust.Shared.GameObjects.BoundUserInterfaceState
{
    public NetEntity? TargetEntity;
    public string Name;
    public string Species;
    public MobState? MobState;
    public float BloodLevel;
    public bool Bleeding;
    public bool ScanMode;
    public List<GehennaTraumaScannerEntry> Wounds;

    public GehennaTraumaScannerUiState()
    {
        TargetEntity = null;
        Name = string.Empty;
        Species = string.Empty;
        MobState = null;
        BloodLevel = float.NaN;
        Bleeding = false;
        ScanMode = false;
        Wounds = new List<GehennaTraumaScannerEntry>();
    }

    public GehennaTraumaScannerUiState(
        NetEntity? targetEntity,
        string name,
        string species,
        MobState? mobState,
        float bloodLevel,
        bool bleeding,
        bool scanMode,
        List<GehennaTraumaScannerEntry> wounds)
    {
        TargetEntity = targetEntity;
        Name = name;
        Species = species;
        MobState = mobState;
        BloodLevel = bloodLevel;
        Bleeding = bleeding;
        ScanMode = scanMode;
        Wounds = wounds;
    }
}
