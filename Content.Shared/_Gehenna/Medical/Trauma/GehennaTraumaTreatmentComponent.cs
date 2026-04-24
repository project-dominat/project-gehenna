using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.FixedPoint;

namespace Content.Shared._Gehenna.Medical.Trauma;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GehennaTraumaTreatmentComponent : Component
{
    [DataField, AutoNetworkedField]
    public GehennaTreatmentKind Treatment = GehennaTreatmentKind.Bandage;

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public bool TreatBurns;

    [DataField, AutoNetworkedField]
    public bool TreatOpenWounds = true;

    [DataField, AutoNetworkedField]
    public int MaxBurnDegree = 2;

    [DataField, AutoNetworkedField]
    public FixedPoint2 BurnSeverityHealing = FixedPoint2.New(5);

    [DataField, AutoNetworkedField]
    public FixedPoint2 BloodRestoreAmount = FixedPoint2.New(15);

    [DataField]
    public SoundSpecifier? BeginSound;

    [DataField]
    public SoundSpecifier? EndSound;
}
