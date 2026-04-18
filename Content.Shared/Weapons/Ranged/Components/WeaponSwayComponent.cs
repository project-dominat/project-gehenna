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
    public const float DefaultBreathAmplitude = 0f;
    public const float DefaultBreathFrequency = 0.35f;
    public const float DefaultBreathHoldMultiplier = 0.25f;
    public const float DefaultBreathHoldMaxDuration = 3.5f;
    public const float DefaultBreathHoldRecoveryRate = 0.75f;
    public const float DefaultBreathHoldExhaustedMultiplier = 1.5f;
    public const float DefaultBreathHoldMovementThreshold = 0.08f;

    // Noise-based sway defaults
    public const int DefaultNoiseOctaves = 3;
    public const float DefaultNoiseLacunarity = 2.7f;
    public const float DefaultNoiseGain = 0.4f;
    public const int DefaultHipFireNoiseOctaves = 4;
    public const float DefaultHipFireNoiseLacunarity = 2.2f;
    public const float DefaultHipFireNoiseGain = 0.5f;

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

    /// <summary>
    /// Low-frequency ADS breathing drift amplitude in tiles.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BreathAmplitude = DefaultBreathAmplitude;

    /// <summary>
    /// Breathing drift cycles per second while deliberately aiming.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BreathFrequency = DefaultBreathFrequency;

    /// <summary>
    /// Sway multiplier applied while the player is holding breath.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BreathHoldMultiplier = DefaultBreathHoldMultiplier;

    /// <summary>
    /// Maximum breath hold duration in seconds.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BreathHoldMaxDuration = DefaultBreathHoldMaxDuration;

    /// <summary>
    /// Breath hold recovery rate in seconds recovered per second.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BreathHoldRecoveryRate = DefaultBreathHoldRecoveryRate;

    /// <summary>
    /// Sway multiplier applied after breath hold is exhausted.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BreathHoldExhaustedMultiplier = DefaultBreathHoldExhaustedMultiplier;

    /// <summary>
    /// Maximum movement factor that still allows breath holding.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BreathHoldMovementThreshold = DefaultBreathHoldMovementThreshold;

    // ── Noise-based sway parameters ──

    /// <summary>
    /// Number of FBm octaves for aimed sway noise. More octaves = finer detail layers.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int NoiseOctaves = DefaultNoiseOctaves;

    /// <summary>
    /// Frequency multiplier between octaves for aimed sway.
    /// Higher = more difference between slow drift and fine tremor.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float NoiseLacunarity = DefaultNoiseLacunarity;

    /// <summary>
    /// Amplitude multiplier between octaves for aimed sway.
    /// Lower = fine detail is subtler relative to main drift.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float NoiseGain = DefaultNoiseGain;

    /// <summary>
    /// Number of FBm octaves for hip-fire sway noise.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int HipFireNoiseOctaves = DefaultHipFireNoiseOctaves;

    /// <summary>
    /// Frequency multiplier between octaves for hip-fire sway.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HipFireNoiseLacunarity = DefaultHipFireNoiseLacunarity;

    /// <summary>
    /// Amplitude multiplier between octaves for hip-fire sway.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HipFireNoiseGain = DefaultHipFireNoiseGain;
}
