using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Gehenna.Medical.Trauma;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedGehennaTraumaSystem))]
public sealed partial class GehennaPainComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public FixedPoint2 Pain = FixedPoint2.Zero;
}
