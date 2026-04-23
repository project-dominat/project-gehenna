using Content.Client.Storage.Visualizers;
using Content.Shared._Gehenna.Prison.Chest;
using Robust.Client.GameObjects;

namespace Content.Client._Gehenna.Prison.Chest;

/// <summary>
///     Overrides the base sprite layer when the false-bottom stash is revealed,
///     running after <see cref="EntityStorageVisualizerSystem"/> so our state wins.
/// </summary>
public sealed class PrisonChestStashVisualizerSystem : VisualizerSystem<PrisonChestStashComponent>
{
    protected override void OnAppearanceChange(
        EntityUid uid,
        PrisonChestStashComponent comp,
        ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!AppearanceSystem.TryGetData<bool>(
                uid,
                PrisonStashVisuals.StashRevealed,
                out var revealed,
                args.Component))
            return;

        var hasDoorLayer = SpriteSystem.LayerMapTryGet((uid, args.Sprite), StorageVisualLayers.Door, out _, false);

        if (revealed)
        {
            // Lock the base layer to the stash-open sprite regardless of
            // whether the main drawer is open or closed.
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), StorageVisualLayers.Base, comp.StashRevealedState);
            if (hasDoorLayer)
                SpriteSystem.LayerSetVisible((uid, args.Sprite), StorageVisualLayers.Door, false);
        }
        else
        {
            // Restore the closed state; EntityStorageVisualizerSystem will
            // take over again on the next drawer open/close event.
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), StorageVisualLayers.Base, comp.StashHiddenState);
            if (hasDoorLayer)
                SpriteSystem.LayerSetVisible((uid, args.Sprite), StorageVisualLayers.Door, true);
        }
    }
}
