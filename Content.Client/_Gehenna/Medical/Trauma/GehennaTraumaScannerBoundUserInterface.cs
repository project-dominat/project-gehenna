using Content.Shared._Gehenna.Medical.Trauma;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Gehenna.Medical.Trauma;

[UsedImplicitly]
public sealed class GehennaTraumaScannerBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private GehennaTraumaScannerWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GehennaTraumaScannerWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (_window == null || message is not GehennaTraumaScannerScannedUserMessage scanned)
            return;

        _window.Populate(scanned.State);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not GehennaTraumaScannerUiState scanned)
            return;

        _window.Populate(scanned);
    }
}
