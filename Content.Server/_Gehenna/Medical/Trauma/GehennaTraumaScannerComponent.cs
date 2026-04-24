using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Gehenna.Medical.Trauma;

[RegisterComponent, AutoGenerateComponentPause]
[Access(typeof(GehennaTraumaScannerSystem))]
public sealed partial class GehennaTraumaScannerComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan ScanDelay = TimeSpan.FromSeconds(0.8);

    [DataField]
    public EntityUid? ScannedEntity;

    [DataField]
    public bool IsAnalyzerActive;

    [DataField]
    public float? MaxScanRange = 2.5f;

    [DataField]
    public SoundSpecifier? ScanningBeginSound;

    [DataField]
    public SoundSpecifier ScanningEndSound = new SoundPathSpecifier("/Audio/Items/Medical/healthscanner.ogg");
}
