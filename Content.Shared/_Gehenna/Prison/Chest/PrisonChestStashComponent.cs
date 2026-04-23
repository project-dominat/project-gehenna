using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Gehenna.Prison.Chest;

/// <summary>
///     Adds a secret false-bottom compartment to a chest drawer.
///     The compartment is toggled via a right-click verb and
///     persists visually until explicitly closed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PrisonChestStashComponent : Component
{
    /// <summary>
    ///     Whether the false bottom is currently revealed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool StashRevealed;

    /// <summary>
    ///     Prototype spawned as the hidden inner storage entity.
    /// </summary>
    [DataField]
    public EntProtoId StashProto = "PrisonChestStashInner";

    /// <summary>
    ///     Legacy stash slot kept for compatibility with existing builds.
    /// </summary>
    [ViewVariables]
    public ContainerSlot StashSlot = default!;

    /// <summary>
    ///     Sprite RSI state shown on the base layer while the stash is revealed.
    /// </summary>
    [DataField]
    public string StashRevealedState = "chestdrawer-stash-open";

    /// <summary>
    ///     Sprite RSI state restored on the base layer when the stash is hidden.
    /// </summary>
    [DataField]
    public string StashHiddenState = "chestdrawer";
}
