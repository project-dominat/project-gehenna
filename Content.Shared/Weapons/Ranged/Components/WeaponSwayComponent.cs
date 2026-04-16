using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Configures client-side virtual aim drift for deliberate aiming and hip-fire.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WeaponSwayComponent : Component
{
    public const float DefaultStillAmplitude = 0.015f;
    public const float DefaultMovingAmplitude = 0.18f;
    public const float DefaultHipFireStillAmplitude = 0.22f;
    public const float DefaultHipFireMovingAmplitude = 0.42f;
    public const float DefaultVelocityForMaxSway = 4.5f;
    public const float DefaultFrequency = 1.35f;
    public const float DefaultHipFireFrequency = 3.15f;
    public const float DefaultInitialInstability = 1.35f;
    public const float DefaultStabilizeTime = 0.45f;
    public const float DefaultMaxSway = 0.35f;
    public const float DefaultHipFireMaxSway = 0.7f;
    public const float DefaultUnwieldedSwayMultiplier = 1.35f;
    public const float DefaultWalkSwayMultiplier = 2f;
    public const float DefaultSprintSwayMultiplier = 4f;
    public const float DefaultAimingMovementPenaltyMultiplier = 0.5f;

    /// <summary>
    /// Aim drift amplitude in tiles while standing still.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StillAmplitude = DefaultStillAmplitude;

    /// <summary>
    /// Aim drift amplitude in tiles while moving at or above <see cref="VelocityForMaxSway"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MovingAmplitude = DefaultMovingAmplitude;

    /// <summary>
    /// Aim drift amplitude in tiles while hip-firing and standing still.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HipFireStillAmplitude = DefaultHipFireStillAmplitude;

    /// <summary>
    /// Aim drift amplitude in tiles while hip-firing and moving.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HipFireMovingAmplitude = DefaultHipFireMovingAmplitude;

    /// <summary>
    /// Movement speed in tiles per second that reaches full movement sway.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float VelocityForMaxSway = DefaultVelocityForMaxSway;

    /// <summary>
    /// Base sway oscillation frequency.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Frequency = DefaultFrequency;

    /// <summary>
    /// Chaotic sway oscillation frequency used while hip-firing.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HipFireFrequency = DefaultHipFireFrequency;

    /// <summary>
    /// Deterministic phase offset for this weapon.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Seed = 0f;

    /// <summary>
    /// Extra sway multiplier immediately after entering aim mode.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float InitialInstability = DefaultInitialInstability;

    /// <summary>
    /// Time in seconds required for initial instability to settle.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StabilizeTime = DefaultStabilizeTime;

    /// <summary>
    /// Maximum allowed sway offset length in tiles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxSway = DefaultMaxSway;

    /// <summary>
    /// Maximum allowed hip-fire sway offset length in tiles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HipFireMaxSway = DefaultHipFireMaxSway;

    /// <summary>
    /// Extra sway multiplier for wieldable weapons fired without being wielded.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float UnwieldedSwayMultiplier = DefaultUnwieldedSwayMultiplier;

    /// <summary>
    /// Sway multiplier while moving with the walk key held.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float WalkSwayMultiplier = DefaultWalkSwayMultiplier;

    /// <summary>
    /// Sway multiplier while moving without the walk key held.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SprintSwayMultiplier = DefaultSprintSwayMultiplier;

    /// <summary>
    /// Scalar applied to the extra movement sway while deliberately aiming.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AimingMovementPenaltyMultiplier = DefaultAimingMovementPenaltyMultiplier;
}
