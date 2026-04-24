using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Shared._Gehenna.Medical.Trauma;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client._Gehenna.Medical.Trauma;

public sealed class GehennaTargetingUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IPlayerManager _player = default!;

    [UISystemDependency] private readonly GehennaTargetingSystem? _targeting = default;

    private GehennaTargetingHud? _hud;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
        gameplayStateLoad.OnScreenUnload += OnScreenUnload;
    }

    public void OnStateEntered(GameplayState state)
    {
        LoadHud();
    }

    public void OnStateExited(GameplayState state)
    {
        UnloadHud();
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);
        SyncHud();
    }

    private void OnScreenLoad()
    {
        LoadHud();
    }

    private void OnScreenUnload()
    {
        UnloadHud();
    }

    private void LoadHud()
    {
        if (_hud != null)
            return;

        _hud = new GehennaTargetingHud();
        _hud.ZonePressed += OnZonePressed;
        UIManager.PopupRoot.AddChild(_hud);
        LayoutContainer.SetAnchorAndMarginPreset(_hud, LayoutContainer.LayoutPreset.BottomRight);
        SyncHud();
    }

    private void UnloadHud()
    {
        if (_hud == null)
            return;

        _hud.ZonePressed -= OnZonePressed;
        UIManager.PopupRoot.RemoveChild(_hud);
        _hud = null;
    }

    private void SyncHud()
    {
        if (_hud == null)
            return;

        if (_player.LocalEntity is not { } player ||
            !EntityManager.TryGetComponent<GehennaTargetingComponent>(player, out var targeting))
        {
            _hud.Visible = false;
            return;
        }

        _hud.Visible = true;
        _hud.SetZone(targeting.TargetZone);
    }

    private void OnZonePressed(GehennaBodyZone zone)
    {
        _targeting?.RequestTargetZone(zone);
    }
}
