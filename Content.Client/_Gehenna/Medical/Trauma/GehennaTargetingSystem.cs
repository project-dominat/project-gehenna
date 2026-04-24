using Content.Shared._Gehenna.Medical.Trauma;
using Robust.Client.Player;

namespace Content.Client._Gehenna.Medical.Trauma;

public sealed class GehennaTargetingSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    public void RequestTargetZone(GehennaBodyZone zone)
    {
        if (_player.LocalEntity is not { } player)
            return;

        RaiseNetworkEvent(new GehennaSetTargetZoneEvent(GetNetEntity(player), zone));
    }
}
