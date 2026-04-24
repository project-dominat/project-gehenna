using Robust.Shared.GameStates;

namespace Content.Shared._Gehenna.Medical.Trauma;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedGehennaTraumaSystem))]
public sealed partial class GehennaTargetingComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public GehennaBodyZone TargetZone = GehennaBodyZone.Torso;
}
