using Robust.Shared.Audio;
using Robust.Shared.GameStates;

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

    [DataField]
    public SoundSpecifier? BeginSound;

    [DataField]
    public SoundSpecifier? EndSound;
}
