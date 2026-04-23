using Robust.Shared.Serialization;

namespace Content.Shared._Gehenna.Prison.Chest;

[Serializable, NetSerializable]
public enum PrisonStashVisuals : byte
{
    /// <summary>
    ///     True when the false bottom is exposed, false when hidden.
    /// </summary>
    StashRevealed,
}
