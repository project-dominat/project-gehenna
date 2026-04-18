#nullable enable
using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Hands;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.Weapons;

public sealed class GunPredictionReproTest : InteractionTest
{
    protected override string PlayerPrototype => "MobHuman";

    private static readonly EntProtoId Kammerer = "WeaponShotgunKammerer";
    private static readonly EntProtoId MobHuman = "MobHuman";

    [Test]
    public async Task KammererWieldAimCycleFirePredictionDoesNotAssert()
    {
        var gunSystem = SEntMan.System<SharedGunSystem>();

        await AddAtmosphere();
        var urist = await SpawnTarget(MobHuman);
        var kammererNet = await PlaceInHands(Kammerer);
        var kammererServer = ToServer(kammererNet);
        var kammererClient = ToClient(kammererNet);

        await Pair.RunSeconds(2f);
        await SetCombatMode(true);

        Assert.That(HasComp<GunRequiresWieldComponent>(kammererNet));
        Assert.That(Comp<WieldableComponent>(kammererNet).Wielded, Is.False);
        Assert.That(gunSystem.GetAmmoCount(kammererServer), Is.GreaterThan(0));

        await Client.WaitPost(() =>
        {
            CEntMan.RaisePredictiveEvent(new RequestUseInHandEvent());
            CEntMan.RaisePredictiveEvent(new RequestStartAimingEvent
            {
                Gun = CEntMan.GetNetEntity(kammererClient)
            });
            CEntMan.RaisePredictiveEvent(new RequestUseInHandEvent());
            CEntMan.RaisePredictiveEvent(new RequestShootEvent
            {
                Gun = CEntMan.GetNetEntity(kammererClient),
                Coordinates = TargetCoords,
                RawCoordinates = TargetCoords,
                Target = urist
            });
        });
        await RunTicks(12);

        Assert.That(Comp<WieldableComponent>(kammererNet).Wielded, Is.True);
    }
}
