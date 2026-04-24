using Robust.Shared.GameStates;

namespace Content.Shared._Gehenna.Medical.Trauma;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedGehennaTraumaSystem))]
public sealed partial class GehennaTraumaComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public List<GehennaWoundData> Wounds = new();

    [DataField]
    public TimeSpan RotDelay = TimeSpan.FromMinutes(7);
}
