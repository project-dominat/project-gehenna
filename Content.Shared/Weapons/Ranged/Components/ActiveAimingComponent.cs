using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Runtime marker for an entity that is deliberately aiming a ranged weapon.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveAimingComponent : Component
{
    /// <summary>
    /// The weapon currently being aimed.
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Weapon;

    /// <summary>
    /// Time when aiming started.
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan StartedAt;
}
