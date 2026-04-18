using System.Numerics;
using Content.Shared.CCVar;
using Content.Shared.Movement.Components;
using Content.Shared.Wieldable.Components;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Input;
using Robust.Shared.Noise;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed class ClientWeaponSwaySystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly InputSystem _inputSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private readonly Dictionary<EntityUid, AimRecoilState> _fallbackRecoil = new();
    private readonly List<EntityUid> _fallbackRemoveBuffer = [];
    private readonly Dictionary<EntityUid, AimBreathState> _breathHold = new();
    private readonly List<EntityUid> _breathRemoveBuffer = [];
    private float _swayMaxMultiplier;

    /// <summary>
    /// Noise generator for the X axis of weapon sway.
    /// Reconfigured per-weapon in TryGetSwayOffset.
    /// </summary>
    private readonly FastNoiseLite _noiseX = new(42);

    /// <summary>
    /// Noise generator for the Y axis of weapon sway.
    /// Uses a different seed to produce independent movement.
    /// </summary>
    private readonly FastNoiseLite _noiseY = new(1337);

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, CCVars.WeaponAimSwayMaxMultiplier, v => _swayMaxMultiplier = v, true);
    }

    public bool TryGetSwayOffset(
        EntityUid user,
        EntityUid weapon,
        bool deliberateAim,
        TimeSpan startedAt,
        out Vector2 offset,
        out Vector2 recoilOffset,
        out float amplitude,
        out float movementFactor)
    {
        offset = Vector2.Zero;
        recoilOffset = GetRecoilOffset(weapon);

        var sway = CompOrNull<WeaponSwayComponent>(weapon);
        var movement = GetMovementSway(user, deliberateAim, sway);
        movementFactor = movement.Factor;

        var stillAmplitude = deliberateAim
            ? sway?.StillAmplitude ?? WeaponSwayComponent.DefaultStillAmplitude
            : sway?.HipFireStillAmplitude ?? WeaponSwayComponent.DefaultHipFireStillAmplitude;
        var movingAmplitude = deliberateAim
            ? sway?.MovingAmplitude ?? WeaponSwayComponent.DefaultMovingAmplitude
            : sway?.HipFireMovingAmplitude ?? WeaponSwayComponent.DefaultHipFireMovingAmplitude;
        var frequency = deliberateAim
            ? sway?.Frequency ?? WeaponSwayComponent.DefaultFrequency
            : sway?.HipFireFrequency ?? WeaponSwayComponent.DefaultHipFireFrequency;
        var seed = sway?.Seed ?? 0f;
        var maxSway = deliberateAim
            ? sway?.MaxSway ?? WeaponSwayComponent.DefaultMaxSway
            : sway?.HipFireMaxSway ?? WeaponSwayComponent.DefaultHipFireMaxSway;
        var maxSwayMultiplier = float.IsFinite(_swayMaxMultiplier) && _swayMaxMultiplier > 0f
            ? _swayMaxMultiplier
            : 1f;
        var breath = GetBreathSample(user, sway, deliberateAim, movementFactor);

        amplitude = MathHelper.Lerp(stillAmplitude, movingAmplitude, movementFactor);
        amplitude *= movement.Multiplier;
        amplitude *= GetBaseSwayMultiplier(weapon);

        if (deliberateAim)
        {
            var initialInstability = sway?.InitialInstability ?? WeaponSwayComponent.DefaultInitialInstability;
            var stabilizeTime = sway?.StabilizeTime ?? WeaponSwayComponent.DefaultStabilizeTime;

            if (stabilizeTime > 0f)
            {
                var aimAge = Math.Max(0f, (float)(_timing.CurTime - startedAt).TotalSeconds);
                var settle = Math.Clamp(aimAge / stabilizeTime, 0f, 1f);
                amplitude *= MathHelper.Lerp(initialInstability, 1f, settle);
            }

            amplitude *= breath.SwayMultiplier;
        }
        else if (TryComp<WieldableComponent>(weapon, out var wieldable) && !wieldable.Wielded)
        {
            amplitude *= sway?.UnwieldedSwayMultiplier ?? WeaponSwayComponent.DefaultUnwieldedSwayMultiplier;
        }

        var swayPenalty = GetSwayPenalty(weapon);
        amplitude += swayPenalty;
        var maxBloomedSway = maxSway * maxSwayMultiplier + swayPenalty;

        // Gehenna edit start — Simplex noise sway replaces sin/cos oscillation
        var t = (float)_timing.CurTime.TotalSeconds;
        var breathOffset = GetBreathOffset(sway, breath, t);

        if (amplitude > 0f)
        {
            // Configure noise generators per weapon state
            int octaves;
            float lacunarity;
            float gain;

            if (deliberateAim)
            {
                octaves = sway?.NoiseOctaves ?? WeaponSwayComponent.DefaultNoiseOctaves;
                lacunarity = sway?.NoiseLacunarity ?? WeaponSwayComponent.DefaultNoiseLacunarity;
                gain = sway?.NoiseGain ?? WeaponSwayComponent.DefaultNoiseGain;
            }
            else
            {
                octaves = sway?.HipFireNoiseOctaves ?? WeaponSwayComponent.DefaultHipFireNoiseOctaves;
                lacunarity = sway?.HipFireNoiseLacunarity ?? WeaponSwayComponent.DefaultHipFireNoiseLacunarity;
                gain = sway?.HipFireNoiseGain ?? WeaponSwayComponent.DefaultHipFireNoiseGain;
            }

            ConfigureNoise(_noiseX, frequency, octaves, lacunarity, gain);
            ConfigureNoise(_noiseY, frequency, octaves, lacunarity, gain);

            // Sample noise at (time, seed_offset) — seed provides per-weapon phase offset
            // GetNoise returns [-1, 1] with FBm fractal layering
            var x = _noiseX.GetNoise(t, seed);
            var y = _noiseY.GetNoise(t, seed + 100f);

            offset = new Vector2(x, y) * amplitude;
        }

        offset += breathOffset;

        if (maxBloomedSway > 0f && offset.Length() > maxBloomedSway)
            offset = offset.Normalized() * maxBloomedSway;
        // Gehenna edit end

        return true;
    }

    /// <summary>
    /// Configures a FastNoiseLite instance for sway generation.
    /// </summary>
    private static void ConfigureNoise(FastNoiseLite noise, float frequency, int octaves, float lacunarity, float gain)
    {
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFrequency(frequency);
        noise.SetFractalOctaves(octaves);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalGain(gain);
    }

    public void ApplyShotFeedback(Entity<GunComponent> gun, Vector2 recoilDirection, bool spreadShot)
    {
        if (recoilDirection == Vector2.Zero)
            return;

        if (TryComp<GunRecoilComponent>(gun.Owner, out var recoil))
        {
            ApplyShotFeedback(gun, recoilDirection, spreadShot, recoil);
            return;
        }

        var state = GetFallbackState(gun.Owner);
        ApplyShotFeedback(
            gun,
            recoilDirection,
            spreadShot,
            null,
            ref state.CurrentSwayPenalty,
            ref state.CurrentRecoilOffset,
            ref state.CurrentRecoilVelocity,
            ref state.TargetRecoilOffset);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var recoilQuery = EntityQueryEnumerator<GunRecoilComponent>();
        while (recoilQuery.MoveNext(out _, out var recoil))
        {
            UpdateRecoilState(
                frameTime,
                recoil.SwayPenaltyDecay * recoil.RecoverySpeed,
                recoil.RecoilApproachRate,
                recoil.RecoilDampingRatio,
                recoil.RecoilRecoveryRate * recoil.RecoverySpeed,
                recoil.MaxRecoilOffset,
                ref recoil.CurrentSwayPenalty,
                ref recoil.CurrentRecoilOffset,
                ref recoil.CurrentRecoilVelocity,
                ref recoil.TargetRecoilOffset);
        }

        _fallbackRemoveBuffer.Clear();
        foreach (var (weapon, state) in _fallbackRecoil)
        {
            if (Deleted(weapon) || HasComp<GunRecoilComponent>(weapon))
            {
                _fallbackRemoveBuffer.Add(weapon);
                continue;
            }

            UpdateRecoilState(
                frameTime,
                GunRecoilComponent.DefaultSwayPenaltyDecay,
                GunRecoilComponent.DefaultRecoilApproachRate,
                GunRecoilComponent.DefaultRecoilDampingRatio,
                GunRecoilComponent.DefaultRecoilRecoveryRate,
                GunRecoilComponent.DefaultMaxRecoilOffset,
                ref state.CurrentSwayPenalty,
                ref state.CurrentRecoilOffset,
                ref state.CurrentRecoilVelocity,
                ref state.TargetRecoilOffset);

            if (state.CurrentSwayPenalty <= 0f &&
                state.CurrentRecoilOffset == Vector2.Zero &&
                state.CurrentRecoilVelocity == Vector2.Zero &&
                state.TargetRecoilOffset == Vector2.Zero)
            {
                _fallbackRemoveBuffer.Add(weapon);
            }
        }

        foreach (var weapon in _fallbackRemoveBuffer)
        {
            _fallbackRecoil.Remove(weapon);
        }

        _breathRemoveBuffer.Clear();
        foreach (var (user, _) in _breathHold)
        {
            if (Deleted(user))
                _breathRemoveBuffer.Add(user);
        }

        foreach (var user in _breathRemoveBuffer)
        {
            _breathHold.Remove(user);
        }
    }

    public float GetMovementFactor(EntityUid user, WeaponSwayComponent? sway = null)
    {
        if (!TryComp<PhysicsComponent>(user, out var physics))
            return 0f;

        var velocity = _physics.GetMapLinearVelocity(user, physics).Length();
        var velocityForMaxSway = sway?.VelocityForMaxSway ?? WeaponSwayComponent.DefaultVelocityForMaxSway;

        if (velocityForMaxSway <= 0f)
            return 0f;

        return Math.Clamp(velocity / velocityForMaxSway, 0f, 1f);
    }

    private MovementSway GetMovementSway(EntityUid user, bool deliberateAim, WeaponSwayComponent? sway)
    {
        var walkMultiplier = sway?.WalkSwayMultiplier ?? WeaponSwayComponent.DefaultWalkSwayMultiplier;
        var sprintMultiplier = sway?.SprintSwayMultiplier ?? WeaponSwayComponent.DefaultSprintSwayMultiplier;
        var multiplier = 1f;
        var factor = 0f;

        if (TryComp<InputMoverComponent>(user, out var mover) && mover.HasDirectionalMovement)
        {
            factor = 1f;
            multiplier = mover.Sprinting ? sprintMultiplier : walkMultiplier;
        }
        else
        {
            factor = GetMovementFactor(user, sway);
            if (factor > 0.05f)
                multiplier = MathHelper.Lerp(1f, sprintMultiplier, factor);
        }

        if (deliberateAim)
        {
            var aimMovementScalar = sway?.AimingMovementPenaltyMultiplier ??
                                    WeaponSwayComponent.DefaultAimingMovementPenaltyMultiplier;
            multiplier = 1f + (multiplier - 1f) * aimMovementScalar;
        }

        return new MovementSway(factor, multiplier);
    }

    private void ApplyShotFeedback(
        Entity<GunComponent> gun,
        Vector2 recoilDirection,
        bool spreadShot,
        GunRecoilComponent recoil)
    {
        ApplyShotFeedback(
            gun,
            recoilDirection,
            spreadShot,
            recoil,
            ref recoil.CurrentSwayPenalty,
            ref recoil.CurrentRecoilOffset,
            ref recoil.CurrentRecoilVelocity,
            ref recoil.TargetRecoilOffset);
    }

    private void ApplyShotFeedback(
        Entity<GunComponent> gun,
        Vector2 recoilDirection,
        bool spreadShot,
        GunRecoilComponent? recoil,
        ref float currentSwayPenalty,
        ref Vector2 currentRecoilOffset,
        ref Vector2 currentRecoilVelocity,
        ref Vector2 targetRecoilOffset)
    {
        var swayPenalty = spreadShot
            ? recoil?.ShotgunSwayPenaltyPerShot ?? GunRecoilComponent.DefaultShotgunSwayPenaltyPerShot
            : recoil?.SwayPenaltyPerShot ?? GunRecoilComponent.DefaultSwayPenaltyPerShot;
        var recoilKickMultiplier = recoil?.RecoilKickMultiplier ?? GunRecoilComponent.DefaultRecoilKickMultiplier;
        var recoilScale = recoil?.RecoilScale ?? GunRecoilComponent.DefaultRecoilScale;
        var recoilImpulseScale = recoilKickMultiplier * recoilScale;
        var maxSwayPenalty = recoil?.MaxSwayPenalty ?? GunRecoilComponent.DefaultMaxSwayPenalty;

        currentSwayPenalty = MathF.Min(maxSwayPenalty, currentSwayPenalty + swayPenalty * recoilKickMultiplier);

        var direction = recoilDirection.Normalized();
        var recoilOffset = spreadShot
            ? recoil?.ShotgunRecoilOffsetPerShot ?? GunRecoilComponent.DefaultShotgunRecoilOffsetPerShot
            : recoil?.RecoilOffsetPerShot ?? GunRecoilComponent.DefaultRecoilOffsetPerShot;
        var maxRecoilOffset = recoil?.MaxRecoilOffset ?? GunRecoilComponent.DefaultMaxRecoilOffset;
        var maxKick = recoil?.MaxKick ?? GunRecoilComponent.DefaultMaxKick;
        var kick = (recoil?.Kick ?? GunRecoilComponent.DefaultKick) *
                   recoilImpulseScale *
                   gun.Comp.CameraRecoilScalarModified;

        if (maxKick > 0f)
            kick = MathF.Min(kick, maxKick);

        var target = targetRecoilOffset + direction * recoilOffset * kick;
        currentRecoilVelocity += direction * kick * recoilOffset * 0.35f;

        var lateral = recoil?.LateralKick ?? GunRecoilComponent.DefaultLateralKick;
        if (lateral != 0f)
        {
            var side = new Vector2(-direction.Y, direction.X);
            var lateralKick = side * _random.NextFloat(-lateral, lateral) * recoilImpulseScale;
            target += lateralKick;
            currentRecoilVelocity += lateralKick * 0.25f;
        }

        if (maxRecoilOffset > 0f && target.Length() > maxRecoilOffset)
            target = target.Normalized() * maxRecoilOffset;

        targetRecoilOffset = target;
    }

    private float GetSwayPenalty(EntityUid weapon)
    {
        if (TryComp<GunRecoilComponent>(weapon, out var recoil))
            return recoil.CurrentSwayPenalty;

        return _fallbackRecoil.TryGetValue(weapon, out var state) ? state.CurrentSwayPenalty : 0f;
    }

    private float GetBaseSwayMultiplier(EntityUid weapon)
    {
        return TryComp<GunRecoilComponent>(weapon, out var recoil)
            ? recoil.BaseSwayMultiplier
            : GunRecoilComponent.DefaultBaseSwayMultiplier;
    }

    private Vector2 GetRecoilOffset(EntityUid weapon)
    {
        if (TryComp<GunRecoilComponent>(weapon, out var recoil))
            return recoil.CurrentRecoilOffset;

        return _fallbackRecoil.TryGetValue(weapon, out var state) ? state.CurrentRecoilOffset : Vector2.Zero;
    }

    private AimRecoilState GetFallbackState(EntityUid weapon)
    {
        if (_fallbackRecoil.TryGetValue(weapon, out var state))
            return state;

        state = new AimRecoilState();
        _fallbackRecoil.Add(weapon, state);
        return state;
    }

    private static void UpdateRecoilState(
        float frameTime,
        float penaltyDecay,
        float approachRate,
        float dampingRatio,
        float recoveryRate,
        float maxRecoilOffset,
        ref float currentSwayPenalty,
        ref Vector2 currentRecoilOffset,
        ref Vector2 currentRecoilVelocity,
        ref Vector2 targetRecoilOffset)
    {
        currentSwayPenalty = MathF.Max(0f, currentSwayPenalty - penaltyDecay * frameTime);
        currentRecoilOffset = DampedSpringTowards(
            currentRecoilOffset,
            targetRecoilOffset,
            ref currentRecoilVelocity,
            approachRate,
            dampingRatio,
            frameTime);
        targetRecoilOffset = MoveTowards(targetRecoilOffset, Vector2.Zero, recoveryRate * frameTime);

        if (maxRecoilOffset > 0f && currentRecoilOffset.Length() > maxRecoilOffset)
        {
            var recoilDirection = currentRecoilOffset.Normalized();
            currentRecoilOffset = recoilDirection * maxRecoilOffset;

            var outwardVelocity = Vector2.Dot(currentRecoilVelocity, recoilDirection);
            if (outwardVelocity > 0f)
                currentRecoilVelocity -= recoilDirection * outwardVelocity;
        }

        if (currentRecoilOffset.LengthSquared() < 0.0001f)
            currentRecoilOffset = Vector2.Zero;

        if (currentRecoilVelocity.LengthSquared() < 0.0001f)
            currentRecoilVelocity = Vector2.Zero;

        if (targetRecoilOffset.LengthSquared() < 0.0001f)
            targetRecoilOffset = Vector2.Zero;
    }

    private static Vector2 DampedSpringTowards(
        Vector2 current,
        Vector2 target,
        ref Vector2 velocity,
        float frequency,
        float dampingRatio,
        float frameTime)
    {
        if (frequency <= 0f)
        {
            velocity = Vector2.Zero;
            return target;
        }

        dampingRatio = Math.Clamp(dampingRatio, 0f, 2f);
        var remaining = Math.Clamp(frameTime, 0f, 0.1f);
        const float MaxStep = 1f / 120f;

        while (remaining > 0f)
        {
            var step = MathF.Min(remaining, MaxStep);
            var displacement = target - current;
            var acceleration = displacement * frequency * frequency - velocity * (2f * dampingRatio * frequency);

            velocity += acceleration * step;
            current += velocity * step;
            remaining -= step;
        }

        return current;
    }

    private static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDelta)
    {
        var delta = target - current;
        var length = delta.Length();

        if (length <= maxDelta || length == 0f)
            return target;

        return current + delta / length * maxDelta;
    }

    private BreathSample GetBreathSample(
        EntityUid user,
        WeaponSwayComponent? sway,
        bool deliberateAim,
        float movementFactor)
    {
        var breathAmplitude = sway?.BreathAmplitude ?? WeaponSwayComponent.DefaultBreathAmplitude;
        if (!deliberateAim || breathAmplitude <= 0f)
            return BreathSample.Disabled;

        var maxDuration = MathF.Max(0f,
            sway?.BreathHoldMaxDuration ?? WeaponSwayComponent.DefaultBreathHoldMaxDuration);
        if (maxDuration <= 0f)
            return BreathSample.Disabled;

        var state = GetBreathState(user, maxDuration);
        var now = _timing.CurTime;
        var delta = state.LastUpdated == TimeSpan.Zero
            ? 0f
            : Math.Clamp((float)(now - state.LastUpdated).TotalSeconds, 0f, 0.25f);
        state.LastUpdated = now;

        var movementThreshold = sway?.BreathHoldMovementThreshold ??
                                WeaponSwayComponent.DefaultBreathHoldMovementThreshold;
        var holdRequested = _inputSystem.CmdStates.GetState(EngineKeyFunctions.Walk) == BoundKeyState.Down;
        var canHold = holdRequested && movementFactor <= movementThreshold;

        if (canHold && !state.Exhausted && state.Remaining > 0f)
        {
            state.Remaining = MathF.Max(0f, state.Remaining - delta);
            if (state.Remaining <= 0f)
                state.Exhausted = true;
        }
        else
        {
            var recoveryRate = MathF.Max(0f,
                sway?.BreathHoldRecoveryRate ?? WeaponSwayComponent.DefaultBreathHoldRecoveryRate);
            state.Remaining = MathF.Min(maxDuration, state.Remaining + recoveryRate * delta);

            if (state.Remaining >= maxDuration * 0.4f)
                state.Exhausted = false;
        }

        var multiplier = 1f;
        if (canHold && !state.Exhausted && state.Remaining > 0f)
        {
            multiplier = Math.Clamp(
                sway?.BreathHoldMultiplier ?? WeaponSwayComponent.DefaultBreathHoldMultiplier,
                0f,
                1f);
        }
        else if (state.Exhausted)
        {
            multiplier = MathF.Max(1f,
                sway?.BreathHoldExhaustedMultiplier ?? WeaponSwayComponent.DefaultBreathHoldExhaustedMultiplier);
        }

        return new BreathSample(true, multiplier);
    }

    private AimBreathState GetBreathState(EntityUid user, float maxDuration)
    {
        if (_breathHold.TryGetValue(user, out var state))
        {
            state.Remaining = Math.Clamp(state.Remaining, 0f, maxDuration);
            return state;
        }

        state = new AimBreathState
        {
            Remaining = maxDuration,
            LastUpdated = _timing.CurTime
        };
        _breathHold.Add(user, state);
        return state;
    }

    private static Vector2 GetBreathOffset(WeaponSwayComponent? sway, BreathSample breath, float time)
    {
        if (!breath.Enabled)
            return Vector2.Zero;

        var amplitude = (sway?.BreathAmplitude ?? WeaponSwayComponent.DefaultBreathAmplitude) *
                        breath.SwayMultiplier;
        var frequency = MathF.Max(0f,
            sway?.BreathFrequency ?? WeaponSwayComponent.DefaultBreathFrequency);

        if (amplitude <= 0f || frequency <= 0f)
            return Vector2.Zero;

        var seed = sway?.Seed ?? 0f;
        var phase = (time + seed * 0.17f) * frequency * MathF.PI * 2f;
        var vertical = MathF.Sin(phase);
        var lateral = MathF.Sin(phase * 0.5f + 1.6f) * 0.35f;

        return new Vector2(lateral, vertical) * amplitude;
    }

    private sealed class AimRecoilState
    {
        public float CurrentSwayPenalty;
        public Vector2 CurrentRecoilOffset;
        public Vector2 CurrentRecoilVelocity;
        public Vector2 TargetRecoilOffset;
    }

    private sealed class AimBreathState
    {
        public float Remaining;
        public bool Exhausted;
        public TimeSpan LastUpdated;
    }

    private readonly record struct MovementSway(float Factor, float Multiplier);

    private readonly record struct BreathSample(bool Enabled, float SwayMultiplier)
    {
        public static readonly BreathSample Disabled = new(false, 1f);
    }
}
