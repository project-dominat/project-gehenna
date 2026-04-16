using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Configures deliberate aiming for a ranged weapon.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedAimingSystem))]
public sealed partial class AimingComponent : Component
{
    public const float DefaultWalkModifier = 0.75f;
    public const float DefaultSprintModifier = 0.55f;
    public const float DefaultEyeOffset = 1.25f;
    public const float DefaultEyeOffsetSpeed = 0.35f;
    public const float DefaultPvsIncrease = 0.2f;

    /// <summary>
    /// Whether this weapon may only be aimed while its user is in combat mode.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RequireCombatMode = true;

    /// <summary>
    /// Multiplier applied to the user's walking speed while aiming.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float WalkModifier = DefaultWalkModifier;

    /// <summary>
    /// Multiplier applied to the user's sprinting speed while aiming.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SprintModifier = DefaultSprintModifier;

    /// <summary>
    /// Camera offset in tiles at the edge of the aim radius.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float EyeOffset = DefaultEyeOffset;

    /// <summary>
    /// Maximum per-update camera offset movement in tiles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float EyeOffsetSpeed = DefaultEyeOffsetSpeed;

    /// <summary>
    /// Server PVS scale increase used to cover the extra aimed camera offset.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float PvsIncrease = DefaultPvsIncrease;

    /// <summary>
    /// Whether the client should draw the aimed crosshair for this weapon.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ShowCrosshair = true;
}
