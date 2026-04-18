using System.Numerics;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using NUnit.Framework;
using Robust.Shared.Map;

namespace Content.Tests.Shared.Weapons.Ranged;

[TestFixture]
public sealed class GunAimValidationTest
{
    [Test]
    public void ComputesDeviationLength()
    {
        var length = GunAimValidation.ComputeDeviationLength(new Vector2(3f, 4f), Vector2.Zero);

        Assert.That(length, Is.EqualTo(5f).Within(0.0001f));
    }

    [Test]
    public void ClampsDeviationToEnvelopeBoundary()
    {
        var clamped = GunAimValidation.ClampToEnvelope(
            Vector2.Zero,
            new Vector2(3f, 4f),
            2f,
            out var wasClamped);

        Assert.Multiple(() =>
        {
            Assert.That(wasClamped, Is.True);
            Assert.That(clamped.Length(), Is.EqualTo(2f).Within(0.0001f));
            Assert.That(clamped.X, Is.EqualTo(1.2f).Within(0.0001f));
            Assert.That(clamped.Y, Is.EqualTo(1.6f).Within(0.0001f));
        });
    }

    [Test]
    public void KeepsShotInsideEnvelopeUnchanged()
    {
        var shot = new Vector2(0.25f, -0.25f);
        var clamped = GunAimValidation.ClampToEnvelope(Vector2.Zero, shot, 1f, out var wasClamped);

        Assert.Multiple(() =>
        {
            Assert.That(wasClamped, Is.False);
            Assert.That(clamped, Is.EqualTo(shot));
        });
    }

    [Test]
    public void HipFireEnvelopeIsWiderThanAdsEnvelope()
    {
        var ads = GunAimValidation.ComputeMaxAllowedDeviation(
            WeaponSwayComponent.DefaultMaxSway,
            0f,
            0f,
            0f);
        var hip = GunAimValidation.ComputeMaxAllowedDeviation(
            WeaponSwayComponent.DefaultHipFireMaxSway,
            0f,
            0f,
            0f);

        Assert.Multiple(() =>
        {
            Assert.That(ads, Is.EqualTo(0.455f).Within(0.0001f));
            Assert.That(hip, Is.EqualTo(0.91f).Within(0.0001f));
            Assert.That(hip, Is.GreaterThan(ads));
        });
    }

    [Test]
    public void ToleranceMultiplierScalesMaxAllowedDeviation()
    {
        var halfTolerance = GunAimValidation.ComputeMaxAllowedDeviation(
            WeaponSwayComponent.DefaultMaxSway,
            0f,
            0f,
            0f,
            GunAimValidation.DefaultTolerance * 0.5f);
        var doubleTolerance = GunAimValidation.ComputeMaxAllowedDeviation(
            WeaponSwayComponent.DefaultMaxSway,
            0f,
            0f,
            0f,
            GunAimValidation.DefaultTolerance * 2f);

        Assert.Multiple(() =>
        {
            Assert.That(halfTolerance, Is.EqualTo(0.2275f).Within(0.0001f));
            Assert.That(doubleTolerance, Is.EqualTo(0.91f).Within(0.0001f));
        });
    }

    [Test]
    public void MovingPlayerAddsHipFireMovementPenalty()
    {
        var movementPenalty = GunAimValidation.ComputeMovementPenalty(
            WeaponSwayComponent.DefaultHipFireMaxSway,
            WeaponSwayComponent.DefaultVelocityForMaxSway / 2f,
            WeaponSwayComponent.DefaultVelocityForMaxSway,
            WeaponSwayComponent.DefaultSprintSwayMultiplier,
            false,
            WeaponSwayComponent.DefaultAimingMovementPenaltyMultiplier);
        var maxDeviation = GunAimValidation.ComputeMaxAllowedDeviation(
            WeaponSwayComponent.DefaultHipFireMaxSway,
            movementPenalty,
            0f,
            0f);

        Assert.Multiple(() =>
        {
            Assert.That(movementPenalty, Is.EqualTo(1.05f).Within(0.0001f));
            Assert.That(maxDeviation, Is.EqualTo(2.275f).Within(0.0001f));
        });
    }

    [Test]
    public void MovingPlayerAdsPenaltyIsReduced()
    {
        var movementPenalty = GunAimValidation.ComputeMovementPenalty(
            WeaponSwayComponent.DefaultMaxSway,
            WeaponSwayComponent.DefaultVelocityForMaxSway / 2f,
            WeaponSwayComponent.DefaultVelocityForMaxSway,
            WeaponSwayComponent.DefaultSprintSwayMultiplier,
            true,
            WeaponSwayComponent.DefaultAimingMovementPenaltyMultiplier);
        var maxDeviation = GunAimValidation.ComputeMaxAllowedDeviation(
            WeaponSwayComponent.DefaultMaxSway,
            movementPenalty,
            0f,
            0f);

        Assert.Multiple(() =>
        {
            Assert.That(movementPenalty, Is.EqualTo(0.2625f).Within(0.0001f));
            Assert.That(maxDeviation, Is.EqualTo(0.79625f).Within(0.0001f));
        });
    }

    [Test]
    public void ClassifyAcceptsZeroDeviation()
    {
        var mapId = new MapId(1);
        var shoot = new MapCoordinates(new Vector2(2f, 3f), mapId);
        var raw = new MapCoordinates(new Vector2(2f, 3f), mapId);

        var result = GunAimValidation.ClassifyMapCoordinates(shoot, raw, 1f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(AimValidationStatus.Accepted));
            Assert.That(result.Coordinates, Is.EqualTo(shoot));
            Assert.That(result.Deviation, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(result.MaxDeviation, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.RejectReason, Is.Null);
        });
    }

    [Test]
    public void ClassifyAcceptsDeviationBelowMax()
    {
        var mapId = new MapId(1);
        var raw = new MapCoordinates(Vector2.Zero, mapId);
        var shoot = new MapCoordinates(new Vector2(0.3f, 0.4f), mapId);

        var result = GunAimValidation.ClassifyMapCoordinates(shoot, raw, 1f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(AimValidationStatus.Accepted));
            Assert.That(result.Coordinates, Is.EqualTo(shoot));
            Assert.That(result.Deviation, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(result.MaxDeviation, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.RejectReason, Is.Null);
        });
    }

    [Test]
    public void ClassifyClampsDeviationAboveMax()
    {
        var mapId = new MapId(1);
        var raw = new MapCoordinates(new Vector2(1f, 1f), mapId);
        var shoot = new MapCoordinates(new Vector2(4f, 5f), mapId);

        var result = GunAimValidation.ClassifyMapCoordinates(shoot, raw, 2f);
        var clampedOffset = result.Coordinates.Position - raw.Position;

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(AimValidationStatus.Clamped));
            Assert.That(result.Coordinates.MapId, Is.EqualTo(mapId));
            Assert.That(result.Coordinates.Position.X, Is.EqualTo(2.2f).Within(0.0001f));
            Assert.That(result.Coordinates.Position.Y, Is.EqualTo(2.6f).Within(0.0001f));
            Assert.That(clampedOffset.Length(), Is.EqualTo(2f).Within(0.0001f));
            Assert.That(Vector2.Dot(Vector2.Normalize(clampedOffset), Vector2.Normalize(shoot.Position - raw.Position)), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.Deviation, Is.EqualTo(5f).Within(0.0001f));
            Assert.That(result.MaxDeviation, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(result.RejectReason, Is.Null);
        });
    }

    [Test]
    public void ClassifyRejectsNaNShootCoordinates()
    {
        var mapId = new MapId(1);
        var raw = new MapCoordinates(Vector2.Zero, mapId);
        var shoot = new MapCoordinates(new Vector2(float.NaN, 0f), mapId);

        var result = GunAimValidation.ClassifyMapCoordinates(shoot, raw, 1f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(AimValidationStatus.Rejected));
            Assert.That(float.IsNaN(result.Deviation), Is.True);
            Assert.That(result.MaxDeviation, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.RejectReason, Does.Contain("shoot"));
        });
    }

    [Test]
    public void ClassifyRejectsInfiniteShootCoordinates()
    {
        var mapId = new MapId(1);
        var raw = new MapCoordinates(Vector2.Zero, mapId);
        var shoot = new MapCoordinates(new Vector2(float.PositiveInfinity, 0f), mapId);

        var result = GunAimValidation.ClassifyMapCoordinates(shoot, raw, 1f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(AimValidationStatus.Rejected));
            Assert.That(float.IsPositiveInfinity(result.Deviation), Is.True);
            Assert.That(result.MaxDeviation, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.RejectReason, Does.Contain("shoot"));
        });
    }

    [Test]
    public void ClassifyRejectsNaNRawCoordinates()
    {
        var mapId = new MapId(1);
        var raw = new MapCoordinates(new Vector2(0f, float.NaN), mapId);
        var shoot = new MapCoordinates(Vector2.One, mapId);

        var result = GunAimValidation.ClassifyMapCoordinates(shoot, raw, 1f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(AimValidationStatus.Rejected));
            Assert.That(float.IsNaN(result.Deviation), Is.True);
            Assert.That(result.MaxDeviation, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.RejectReason, Does.Contain("raw"));
        });
    }

    [Test]
    public void ClassifyRejectsMismatchedMaps()
    {
        var raw = new MapCoordinates(Vector2.Zero, new MapId(1));
        var shoot = new MapCoordinates(Vector2.One, new MapId(2));

        var result = GunAimValidation.ClassifyMapCoordinates(shoot, raw, 1f);

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(AimValidationStatus.Rejected));
            Assert.That(result.Deviation, Is.EqualTo(Vector2.One.Length()).Within(0.0001f));
            Assert.That(result.MaxDeviation, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.RejectReason, Does.Contain("different maps"));
        });
    }
}
