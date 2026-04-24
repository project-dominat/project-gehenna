using Content.Shared._Gehenna.Medical.Trauma;

namespace Content.Server._Gehenna.Medical.Trauma;

public sealed class GehennaTargetingSystem : EntitySystem
{
    [Dependency] private readonly SharedGehennaTraumaSystem _trauma = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<GehennaSetTargetZoneEvent>(OnSetTargetZone);
    }

    private void OnSetTargetZone(GehennaSetTargetZoneEvent ev, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } player)
            return;

        var target = GetEntity(ev.Target);
        if (target != player)
            return;

        if (!HasComp<GehennaTargetingComponent>(target))
            return;

        _trauma.SetTargetZone(target, ev.Zone);
    }
}
