using Content.Shared.Wieldable;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Applies an accuracy bonus upon being wielded and modifies recoil parameters.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedWieldableSystem))]
public sealed partial class GunWieldBonusComponent : Component
{
    /// <summary>
    /// Angle bonus applied upon being wielded.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("minAngle"), AutoNetworkedField]
    public Angle MinAngle = Angle.FromDegrees(-43);

    /// <summary>
    /// Angle bonus applied upon being wielded.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxAngle"), AutoNetworkedField]
    public Angle MaxAngle = Angle.FromDegrees(-43);

    [DataField, AutoNetworkedField]
    public Angle AngleIncrease = Angle.Zero;

    [DataField, AutoNetworkedField]
    public Angle AngleDecay = Angle.Zero;

    /// <summary>
    /// Recoil modifiers applied when being wielded.
    /// These values are multiplied with corresponding parameters in GunRecoilComponent.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float KickMultiplier = 1f;

    /// <summary>
    /// Sway multiplier applied when being wielded.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SwayMultiplier = 1f;

    /// <summary>
    /// Recoil scale multiplier applied when being wielded.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RecoilScaleMultiplier = 1f;

    /// <summary>
    /// Recovery speed multiplier applied when being wielded.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RecoverySpeedMultiplier = 1f;

    /// <summary>
    /// Accuracy modifier applied when being wielded.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AccuracyMultiplier = 1f;

    /// <summary>
    /// Custom message for examining the wield bonus.
    /// </summary>
    [DataField]
    public string? WieldBonusExamineMessage;

    /// <summary>
    /// Custom message for displaying the wield bonus in the hotbar.
    /// </summary>
    [DataField]
    public string? WieldBonusHotbarMessage;
}
