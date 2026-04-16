using System.Numerics;
using Content.Shared.Movement.Components;
using Content.Shared.Wieldable.Components;
using Content.Shared.Weapons.Ranged.Components;
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

    private readonly Dictionary<EntityUid, AimRecoilState> _fallbackRecoil = new();
    private readonly List<EntityUid> _fallbackRemoveBuffer = [];

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

        amplitude = MathHelper.Lerp(stillAmplitude, movingAmplitude, movementFactor);
        amplitude *= movement.Multiplier;
        amplitude *= GetBaseSwayMultiplier(weapon);

        if (deliberateAim)
        {
            var initialInstability = sway?.InitialInstability ?? WeaponSwayComponent.DefaultInitialInstability;
            var stabilizeTime = sway?.StabilizeTime ?? WeaponSwayComponent.DefaultStabilizeTime;

            if (stabilizeTime > 0f)
            {
                var aimAge = Math.Max(0f, (float) (_timing.CurTime - startedAt).TotalSeconds);
                var settle = Math.Clamp(aimAge / stabilizeTime, 0f, 1f);
                amplitude *= MathHelper.Lerp(initialInstability, 1f, settle);
            }
        }
        else if (TryComp<WieldableComponent>(weapon, out var wieldable) && !wieldable.Wielded)
        {
            amplitude *= sway?.UnwieldedSwayMultiplier ?? WeaponSwayComponent.DefaultUnwieldedSwayMultiplier;
        }

        amplitude += GetSwayPenalty(weapon);

        if (amplitude <= 0f)
            return true;

        var t = (float) _timing.CurTime.TotalSeconds;
        var x = MathF.Sin((t * frequency + seed) * MathF.Tau) +
                MathF.Sin((t * frequency * 0.47f + seed + 0.19f) * MathF.Tau) * 0.35f;
        var y = MathF.Cos((t * frequency * 0.83f + seed + 0.37f) * MathF.Tau) +
                MathF.Sin((t * frequency * 0.31f + seed + 0.61f) * MathF.Tau) * 0.45f;

        if (!deliberateAim)
        {
            x += MathF.Sin((t * frequency * 1.91f + seed + 0.73f) * MathF.Tau) * 0.6f;
            y += MathF.Cos((t * frequency * 1.67f + seed + 0.11f) * MathF.Tau) * 0.55f;
        }

        offset = new Vector2(x, y);
        if (offset.LengthSquared() > 0f && deliberateAim)
            offset = offset.Normalized() * amplitude;
        else
            offset *= amplitude * 0.45f;

        if (maxSway > 0f && offset.Length() > maxSway)
            offset = offset.Normalized() * maxSway;

        return true;
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
                recoil.RecoilRecoveryRate * recoil.RecoverySpeed,
                ref recoil.CurrentSwayPenalty,
                ref recoil.CurrentRecoilOffset,
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
                GunRecoilComponent.DefaultRecoilRecoveryRate,
                ref state.CurrentSwayPenalty,
                ref state.CurrentRecoilOffset,
                ref state.TargetRecoilOffset);

            if (state.CurrentSwayPenalty <= 0f &&
                state.CurrentRecoilOffset == Vector2.Zero &&
                state.TargetRecoilOffset == Vector2.Zero)
            {
                _fallbackRemoveBuffer.Add(weapon);
            }
        }

        foreach (var weapon in _fallbackRemoveBuffer)
        {
            _fallbackRecoil.Remove(weapon);
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
            ref recoil.TargetRecoilOffset);
    }

    private void ApplyShotFeedback(
        Entity<GunComponent> gun,
        Vector2 recoilDirection,
        bool spreadShot,
        GunRecoilComponent? recoil,
        ref float currentSwayPenalty,
        ref Vector2 currentRecoilOffset,
        ref Vector2 targetRecoilOffset)
    {
        var swayPenalty = spreadShot
            ? recoil?.ShotgunSwayPenaltyPerShot ?? GunRecoilComponent.DefaultShotgunSwayPenaltyPerShot
            : recoil?.SwayPenaltyPerShot ?? GunRecoilComponent.DefaultSwayPenaltyPerShot;
        var recoilKickMultiplier = recoil?.RecoilKickMultiplier ?? GunRecoilComponent.DefaultRecoilKickMultiplier;
        var maxSwayPenalty = recoil?.MaxSwayPenalty ?? GunRecoilComponent.DefaultMaxSwayPenalty;

        currentSwayPenalty = MathF.Min(maxSwayPenalty, currentSwayPenalty + swayPenalty * recoilKickMultiplier);

        var direction = recoilDirection.Normalized();
        var recoilOffset = spreadShot
            ? recoil?.ShotgunRecoilOffsetPerShot ?? GunRecoilComponent.DefaultShotgunRecoilOffsetPerShot
            : recoil?.RecoilOffsetPerShot ?? GunRecoilComponent.DefaultRecoilOffsetPerShot;
        var maxRecoilOffset = recoil?.MaxRecoilOffset ?? GunRecoilComponent.DefaultMaxRecoilOffset;
        var maxKick = recoil?.MaxKick ?? GunRecoilComponent.DefaultMaxKick;
        var kick = (recoil?.Kick ?? GunRecoilComponent.DefaultKick) *
                   recoilKickMultiplier *
                   gun.Comp.CameraRecoilScalarModified;

        if (maxKick > 0f)
            kick = MathF.Min(kick, maxKick);

        var target = targetRecoilOffset + direction * recoilOffset * kick;
        var lateral = recoil?.LateralKick ?? GunRecoilComponent.DefaultLateralKick;
        if (lateral != 0f)
        {
            var side = new Vector2(-direction.Y, direction.X);
            target += side * _random.NextFloat(-lateral, lateral) * recoilKickMultiplier;
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
        float recoveryRate,
        ref float currentSwayPenalty,
        ref Vector2 currentRecoilOffset,
        ref Vector2 targetRecoilOffset)
    {
        currentSwayPenalty = MathF.Max(0f, currentSwayPenalty - penaltyDecay * frameTime);
        currentRecoilOffset = SpringTowards(currentRecoilOffset, targetRecoilOffset, approachRate, frameTime);
        targetRecoilOffset = MoveTowards(targetRecoilOffset, Vector2.Zero, recoveryRate * frameTime);

        if (currentRecoilOffset.LengthSquared() < 0.0001f)
            currentRecoilOffset = Vector2.Zero;

        if (targetRecoilOffset.LengthSquared() < 0.0001f)
            targetRecoilOffset = Vector2.Zero;
    }

    private static Vector2 SpringTowards(Vector2 current, Vector2 target, float rate, float frameTime)
    {
        if (rate <= 0f)
            return target;

        var t = Math.Clamp(1f - MathF.Exp(-rate * frameTime), 0f, 1f);
        return Vector2.Lerp(current, target, t);
    }

    private static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDelta)
    {
        var delta = target - current;
        var length = delta.Length();

        if (length <= maxDelta || length == 0f)
            return target;

        return current + delta / length * maxDelta;
    }

    private sealed class AimRecoilState
    {
        public float CurrentSwayPenalty;
        public Vector2 CurrentRecoilOffset;
        public Vector2 TargetRecoilOffset;
    }

    private readonly record struct MovementSway(float Factor, float Multiplier);
}
