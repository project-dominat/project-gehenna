#nullable enable
using System.Linq;
using System.Numerics;
using System.Reflection;
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.CCVar;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests.Weapons;

public sealed class GunAimTelemetryTest : InteractionTest
{
    protected override string PlayerPrototype => "MobHuman";

    private const string GunPrototype = "WeaponDisabler";
    private const int ClampThreshold = 2;
    private const float TelemetryWindowSeconds = 30f;

    [Test]
    public async Task ClampThresholdWarningAndStatusCleanup()
    {
        var gun = await PrepareGun();
        var catcher = await AddAimValidationLogCatcher();
        var failureLevel = AllowServerWarnings();
        var clampedCoords = new NetCoordinates(PlayerCoords.NetEntity, PlayerCoords.Position + new Vector2(20f, 0f));

        try
        {
            await Server.WaitPost(() =>
            {
                for (var i = 0; i < ClampThreshold + 1; i++)
                {
                    RaiseShootRequest(gun, clampedCoords, PlayerCoords);
                }
            });

            await RunTicks(5);

            await Server.WaitAssertion(() =>
            {
                Assert.That(SGun.TryGetAimTelemetryStats(ServerSession.UserId, out var stats), Is.True);
                Assert.That(stats.ClampedCount, Is.EqualTo(ClampThreshold + 1));
                Assert.That(stats.AcceptedCount, Is.Zero);
                Assert.That(stats.RejectedCount, Is.Zero);
                Assert.That(GetCaughtLogCount(catcher), Is.GreaterThanOrEqualTo(1));
            });

            await Server.WaitPost(() => RaiseAimTelemetryStatusChanged(SessionStatus.Zombie));
            await Server.WaitAssertion(() =>
            {
                Assert.That(SGun.TryGetAimTelemetryStats(ServerSession.UserId, out _), Is.False);
            });
        }
        finally
        {
            await RemoveAimValidationLogCatcher(catcher);
            RestoreServerFailureLevel(failureLevel);
        }
    }

    [Test]
    public async Task RejectedAimValidationIncrementsAndDoesNotShoot()
    {
        var gun = await PrepareGun();
        var gunServer = ToServer(gun);
        var nanCoordinates = new NetCoordinates(PlayerCoords.NetEntity, new Vector2(float.NaN, 0f));
        var ammoBefore = SGun.GetAmmoCount(gunServer);

        var failureLevel = AllowServerWarnings();
        try
        {
            await Server.WaitPost(() => RaiseShootRequest(gun, nanCoordinates, PlayerCoords));

            await RunTicks(5);

            await Server.WaitAssertion(() =>
            {
                Assert.That(SGun.TryGetAimTelemetryStats(ServerSession.UserId, out var stats), Is.True);
                Assert.That(stats.RejectedCount, Is.EqualTo(1));
                Assert.That(stats.AcceptedCount, Is.Zero);
                Assert.That(stats.ClampedCount, Is.Zero);
                Assert.That(SGun.GetAmmoCount(gunServer), Is.EqualTo(ammoBefore));
            });
        }
        finally
        {
            RestoreServerFailureLevel(failureLevel);
        }
    }

    private async Task<NetEntity> PrepareGun()
    {
        await Server.WaitPost(() =>
        {
            Server.CfgMan.SetCVar(CCVars.WeaponAimTelemetryEnabled, true);
            Server.CfgMan.SetCVar(CCVars.WeaponAimTelemetryThreshold, ClampThreshold);
            Server.CfgMan.SetCVar(CCVars.WeaponAimTelemetryWindowSeconds, TelemetryWindowSeconds);
            Server.CfgMan.SetCVar(CCVars.WeaponAimEnvelopeTolerance, 1.0f);
        });

        var gun = await PlaceInHands(GunPrototype);
        await SetCombatMode(true);
        return gun;
    }

    private void RaiseShootRequest(NetEntity gun, NetCoordinates coordinates, NetCoordinates rawCoordinates)
    {
        var request = new RequestShootEvent
        {
            Gun = gun,
            Coordinates = coordinates,
            RawCoordinates = rawCoordinates,
        };

        var sessionMessageType = typeof(EntityEventArgs)
            .Assembly
            .GetType("Robust.Shared.GameObjects.EntitySessionMessage`1")!
            .MakeGenericType(typeof(RequestShootEvent));
        var sessionMessage = Activator.CreateInstance(
            sessionMessageType,
            new EntitySessionEventArgs(ServerSession),
            request)!;

        SEntMan.EventBus.RaiseEvent(EventSource.Network, sessionMessage);
    }

    private void RaiseAimTelemetryStatusChanged(SessionStatus status)
    {
        var eventArgs = new SessionStatusEventArgs(ServerSession, SessionStatus.InGame, status);
        typeof(SharedGunSystem)
            .GetMethod("OnPlayerStatusChanged", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(SGun, [null, eventArgs]);
    }

    private async Task<LogCatcher> AddAimValidationLogCatcher()
    {
        var catcher = new LogCatcher();
        await Server.WaitPost(() =>
        {
            Server.ResolveDependency<ILogManager>()
                .GetSawmill(SharedGunSystem.AimValidationSawmillName)
                .AddHandler(catcher);
        });
        return catcher;
    }

    private async Task RemoveAimValidationLogCatcher(LogCatcher catcher)
    {
        await Server.WaitPost(() =>
        {
            Server.ResolveDependency<ILogManager>()
                .GetSawmill(SharedGunSystem.AimValidationSawmillName)
                .RemoveHandler(catcher);
        });
    }

    private LogLevel? AllowServerWarnings()
    {
        var failureLevel = Pair.ServerLogHandler.FailureLevel;
        Pair.ServerLogHandler.FailureLevel = LogLevel.Error;
        return failureLevel;
    }

    private void RestoreServerFailureLevel(LogLevel? failureLevel)
    {
        Pair.ServerLogHandler.FailureLevel = failureLevel;
    }

    private static int GetCaughtLogCount(LogCatcher catcher)
    {
        var logs = typeof(LogCatcher).GetProperty("CaughtLogs")!.GetValue(catcher)!;
        return (int) logs.GetType().GetProperty("Count")!.GetValue(logs)!;
    }
}
