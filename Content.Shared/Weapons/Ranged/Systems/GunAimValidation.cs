using System.Numerics;
using Robust.Shared.Map;

namespace Content.Shared.Weapons.Ranged.Systems;

public enum AimValidationStatus : byte
{
    Accepted,
    Clamped,
    Rejected
}

public readonly struct AimValidationResult
{
    public readonly AimValidationStatus Status;
    public readonly EntityCoordinates Coordinates;
    public readonly float Deviation;
    public readonly float MaxDeviation;
    public readonly string? RejectReason;

    public AimValidationResult(
        AimValidationStatus status,
        EntityCoordinates coordinates,
        float deviation,
        float maxDeviation,
        string? rejectReason)
    {
        Status = status;
        Coordinates = coordinates;
        Deviation = deviation;
        MaxDeviation = maxDeviation;
        RejectReason = rejectReason;
    }
}

public readonly record struct GunAimValidationRecordedEvent(AimValidationStatus Status);

public static class GunAimValidation
{
    public const float DefaultTolerance = 1.3f;
    public const float MinAllowedDeviation = 0.1f;
    public const float DeviationEpsilon = 0.001f;
    // Gehenna edit start - lateral-only aim envelope
    public const float AimAxisEpsilon = 0.0001f;
    // Gehenna edit end
    // Gehenna edit start - distance-scaled bullet spread
    public const float AimDistanceSpreadReference = 1f;
    public const float AimDistanceSpreadGainPerTile = 0.2f;
    public const float AimDistanceSpreadMaxMultiplier = 4f;
    // Gehenna edit end

    public static Vector2 ComputeDeviation(Vector2 shootPosition, Vector2 rawPosition)
    {
        return shootPosition - rawPosition;
    }

    public static float ComputeDeviationLength(Vector2 shootPosition, Vector2 rawPosition)
    {
        return ComputeDeviation(shootPosition, rawPosition).Length();
    }

    public static float ComputeMovementPenalty(
        float maxSway,
        float velocity,
        float velocityForMaxSway,
        float sprintSwayMultiplier,
        bool isAiming,
        float aimingMovementPenaltyMultiplier)
    {
        if (velocityForMaxSway <= 0f)
            return 0f;

        var moveFactor = Math.Clamp(velocity / velocityForMaxSway, 0f, 1f);
        var moveMultiplier = Lerp(1f, sprintSwayMultiplier, moveFactor);

        if (isAiming)
            moveMultiplier = 1f + (moveMultiplier - 1f) * aimingMovementPenaltyMultiplier;

        return maxSway * (moveMultiplier - 1f);
    }

    public static float ComputeMaxAllowedDeviation(
        float maxSway,
        float movementPenalty,
        float bloom,
        float recoilEstimate,
        float tolerance = DefaultTolerance)
    {
        var total = (maxSway + movementPenalty + bloom + recoilEstimate) * tolerance;
        return MathF.Max(total, MinAllowedDeviation);
    }

    // Gehenna edit start - distance-scaled bullet spread
    public static float ComputeAimDistanceSpreadMultiplier(float aimDistance)
    {
        if (!float.IsFinite(aimDistance) || aimDistance <= AimDistanceSpreadReference)
            return 1f;

        var extraDistance = aimDistance - AimDistanceSpreadReference;
        return Math.Clamp(
            1f + extraDistance * AimDistanceSpreadGainPerTile,
            1f,
            AimDistanceSpreadMaxMultiplier);
    }

    public static float ComputeAimDistanceSpreadMultiplier(Vector2 originPosition, Vector2 rawPosition)
    {
        if (!IsFinite(originPosition) || !IsFinite(rawPosition))
            return 1f;

        return ComputeAimDistanceSpreadMultiplier((rawPosition - originPosition).Length());
    }

    public static float ComputeLateralProjectileSpreadRadius(float aimDistance, Angle spread)
    {
        if (!float.IsFinite(aimDistance) || aimDistance <= 0f)
            return 0f;

        var halfAngle = Math.Min(Math.Abs(spread.Theta) * 0.5, Math.PI / 2 - 0.001);
        if (halfAngle <= 0f)
            return 0f;

        return (float) Math.Tan(halfAngle) * MathF.Max(aimDistance, AimDistanceSpreadReference);
    }
    // Gehenna edit end

    public static Vector2 ClampToEnvelope(
        Vector2 rawPosition,
        Vector2 shootPosition,
        float maxDeviation,
        out bool clamped)
    {
        var deviation = ComputeDeviation(shootPosition, rawPosition);
        var deviationLength = deviation.Length();

        if (deviationLength < DeviationEpsilon || deviationLength <= maxDeviation)
        {
            clamped = false;
            return shootPosition;
        }

        clamped = true;
        return rawPosition + deviation / deviationLength * MathF.Max(maxDeviation, 0f);
    }

    // Gehenna edit start - lateral-only aim envelope
    public static bool TryGetAimAxes(
        Vector2 originPosition,
        Vector2 rawPosition,
        Vector2? fallbackDirection,
        out Vector2 forwardAxis,
        out Vector2 lateralAxis)
    {
        forwardAxis = rawPosition - originPosition;
        lateralAxis = default;

        if (!IsUsableAxis(forwardAxis))
        {
            if (fallbackDirection is not { } fallback || !IsUsableAxis(fallback))
                return false;

            forwardAxis = fallback;
        }

        forwardAxis = Vector2.Normalize(forwardAxis);
        lateralAxis = new Vector2(-forwardAxis.Y, forwardAxis.X);
        return true;
    }

    public static Vector2 ProjectOffsetToLateralAxis(Vector2 offset, Vector2 lateralAxis)
    {
        if (!IsFinite(offset) || !IsUsableAxis(lateralAxis))
            return Vector2.Zero;

        lateralAxis = Vector2.Normalize(lateralAxis);
        return lateralAxis * Vector2.Dot(offset, lateralAxis);
    }

    public static (AimValidationStatus Status, MapCoordinates Coordinates, float Deviation, float MaxDeviation, string? RejectReason)
        ClassifyLateralMapCoordinates(
            MapCoordinates shootCoords,
            MapCoordinates rawCoords,
            MapCoordinates originCoords,
            float maxDeviation,
            Vector2? fallbackDirection = null)
    {
        var effectiveMaxDeviation = MathF.Max(maxDeviation, 0f);
        var deviation = ComputeDeviation(shootCoords.Position, rawCoords.Position);
        var deviationLength = deviation.Length();

        if (!IsFinite(shootCoords.Position))
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "shoot coordinates contain non-finite values");
        }

        if (!IsFinite(rawCoords.Position))
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "raw coordinates contain non-finite values");
        }

        if (!IsFinite(originCoords.Position))
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "origin coordinates contain non-finite values");
        }

        if (shootCoords.MapId == MapId.Nullspace)
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "shoot coordinates resolved to nullspace");
        }

        if (rawCoords.MapId == MapId.Nullspace)
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "raw coordinates resolved to nullspace");
        }

        if (originCoords.MapId == MapId.Nullspace)
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "origin coordinates resolved to nullspace");
        }

        if (shootCoords.MapId != rawCoords.MapId ||
            shootCoords.MapId != originCoords.MapId)
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "shoot, raw, and origin coordinates resolved to different maps");
        }

        if (!TryGetAimAxes(originCoords.Position, rawCoords.Position, fallbackDirection, out var forwardAxis, out var lateralAxis))
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "aim axis could not be resolved");
        }

        var lateralDeviation = Vector2.Dot(deviation, lateralAxis);
        var forwardDeviation = Vector2.Dot(deviation, forwardAxis);
        var clampedLateral = Math.Clamp(lateralDeviation, -effectiveMaxDeviation, effectiveMaxDeviation);
        var clampedForward = MathF.Abs(forwardDeviation) > DeviationEpsilon;
        var clampedSide = MathF.Abs(lateralDeviation - clampedLateral) > DeviationEpsilon;

        if (!clampedForward && !clampedSide)
        {
            return (
                AimValidationStatus.Accepted,
                shootCoords,
                deviationLength,
                effectiveMaxDeviation,
                null);
        }

        var clampedPosition = rawCoords.Position + lateralAxis * clampedLateral;

        return (
            AimValidationStatus.Clamped,
            new MapCoordinates(clampedPosition, rawCoords.MapId),
            deviationLength,
            effectiveMaxDeviation,
            null);
    }
    // Gehenna edit end

    public static (AimValidationStatus Status, MapCoordinates Coordinates, float Deviation, float MaxDeviation, string? RejectReason)
        ClassifyMapCoordinates(MapCoordinates shootCoords, MapCoordinates rawCoords, float maxDeviation)
    {
        var effectiveMaxDeviation = MathF.Max(maxDeviation, 0f);
        var deviationLength = ComputeDeviationLength(shootCoords.Position, rawCoords.Position);

        if (!IsFinite(shootCoords.Position))
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "shoot coordinates contain non-finite values");
        }

        if (!IsFinite(rawCoords.Position))
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "raw coordinates contain non-finite values");
        }

        if (shootCoords.MapId == MapId.Nullspace)
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "shoot coordinates resolved to nullspace");
        }

        if (rawCoords.MapId == MapId.Nullspace)
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "raw coordinates resolved to nullspace");
        }

        if (shootCoords.MapId != rawCoords.MapId)
        {
            return (
                AimValidationStatus.Rejected,
                MapCoordinates.Nullspace,
                deviationLength,
                effectiveMaxDeviation,
                "shoot and raw coordinates resolved to different maps");
        }

        if (deviationLength <= effectiveMaxDeviation)
        {
            return (
                AimValidationStatus.Accepted,
                shootCoords,
                deviationLength,
                effectiveMaxDeviation,
                null);
        }

        var clampedPosition = ClampToEnvelope(rawCoords.Position, shootCoords.Position, effectiveMaxDeviation, out _);

        return (
            AimValidationStatus.Clamped,
            new MapCoordinates(clampedPosition, rawCoords.MapId),
            deviationLength,
            effectiveMaxDeviation,
            null);
    }

    private static float Lerp(float from, float to, float by)
    {
        return from + (to - from) * by;
    }

    private static bool IsFinite(Vector2 position)
    {
        return float.IsFinite(position.X) && float.IsFinite(position.Y);
    }

    // Gehenna edit start - lateral-only aim envelope
    private static bool IsUsableAxis(Vector2 axis)
    {
        return IsFinite(axis) && axis.LengthSquared() > AimAxisEpsilon;
    }
    // Gehenna edit end
}
