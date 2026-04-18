using Content.Shared.Weapons.Ranged.Systems;
using Prometheus;

namespace Content.Server.Weapons.Ranged.Systems;

public sealed class GunAimPrometheusSystem : EntitySystem
{
    private const string AcceptedResult = "accepted";
    private const string ClampedResult = "clamped";
    private const string RejectedResult = "rejected";

    private static readonly Counter GunAimShots = Metrics.CreateCounter(
        "gun_aim_shots_total",
        "Total gun aim validation results by status.",
        new CounterConfiguration
        {
            LabelNames = new[] { "result" },
        });

    public override void Initialize()
    {
        base.Initialize();

        GunAimShots.WithLabels(AcceptedResult);
        GunAimShots.WithLabels(ClampedResult);
        GunAimShots.WithLabels(RejectedResult);

        SubscribeLocalEvent<GunAimValidationRecordedEvent>(OnRecorded);
    }

    private void OnRecorded(GunAimValidationRecordedEvent ev)
    {
        GunAimShots.WithLabels(GetResultLabel(ev.Status)).Inc();
    }

    private static string GetResultLabel(AimValidationStatus status)
    {
        return status switch
        {
            AimValidationStatus.Accepted => AcceptedResult,
            AimValidationStatus.Clamped => ClampedResult,
            AimValidationStatus.Rejected => RejectedResult,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };
    }
}
