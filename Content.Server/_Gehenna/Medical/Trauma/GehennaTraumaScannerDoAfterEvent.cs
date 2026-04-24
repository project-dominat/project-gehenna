using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Server._Gehenna.Medical.Trauma;

[Serializable, NetSerializable]
public sealed partial class GehennaTraumaScannerDoAfterEvent : SimpleDoAfterEvent
{
}
