using Content.Server.Body.Systems;
using Content.Shared._Gehenna.Medical.Trauma;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Gehenna.Medical.Trauma;

public sealed class GehennaTraumaScannerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGehennaTraumaSystem _trauma = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GehennaTraumaScannerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<GehennaTraumaScannerComponent, GehennaTraumaScannerDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<GehennaTraumaScannerComponent, EntGotInsertedIntoContainerMessage>(OnInsertedIntoContainer);
        SubscribeLocalEvent<GehennaTraumaScannerComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<GehennaTraumaScannerComponent, DroppedEvent>(OnDropped);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GehennaTraumaScannerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var scanner, out var transform))
        {
            if (scanner.NextUpdate > _timing.CurTime || scanner.ScannedEntity is not { } patient)
                continue;

            if (Deleted(patient))
            {
                StopAnalyzing((uid, scanner), patient);
                continue;
            }

            scanner.NextUpdate = _timing.CurTime + scanner.UpdateInterval;

            if (scanner.MaxScanRange != null &&
                !_transform.InRange(Transform(patient).Coordinates, transform.Coordinates, scanner.MaxScanRange.Value))
            {
                PauseAnalyzing((uid, scanner), patient);
                continue;
            }

            scanner.IsAnalyzerActive = true;
            UpdateScannedUser(uid, patient, true);
        }
    }

    private void OnAfterInteract(Entity<GehennaTraumaScannerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !HasComp<MobStateComponent>(args.Target))
            return;

        _audio.PlayPvs(ent.Comp.ScanningBeginSound, ent);
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, ent.Comp.ScanDelay, new GehennaTraumaScannerDoAfterEvent(), ent, target: args.Target, used: ent)
        {
            NeedHand = true,
            BreakOnMove = true,
        });
    }

    private void OnDoAfter(Entity<GehennaTraumaScannerComponent> ent, ref GehennaTraumaScannerDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        _audio.PlayPvs(ent.Comp.ScanningEndSound, ent);
        OpenUi(args.User, ent);
        BeginAnalyzing(ent, args.Target.Value);
        args.Handled = true;
    }

    private void OnInsertedIntoContainer(Entity<GehennaTraumaScannerComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (ent.Comp.ScannedEntity != null)
            _toggle.TryDeactivate(ent.Owner);
    }

    private void OnToggled(Entity<GehennaTraumaScannerComponent> ent, ref ItemToggledEvent args)
    {
        if (!args.Activated && ent.Comp.ScannedEntity is { } patient)
            StopAnalyzing(ent, patient);
    }

    private void OnDropped(Entity<GehennaTraumaScannerComponent> ent, ref DroppedEvent args)
    {
        if (ent.Comp.ScannedEntity != null)
            _toggle.TryDeactivate(ent.Owner);
    }

    private void OpenUi(EntityUid user, EntityUid scanner)
    {
        if (_ui.HasUi(scanner, GehennaTraumaScannerUiKey.Key))
            _ui.OpenUi(scanner, GehennaTraumaScannerUiKey.Key, user);
    }

    private void BeginAnalyzing(Entity<GehennaTraumaScannerComponent> scanner, EntityUid patient)
    {
        scanner.Comp.ScannedEntity = patient;
        _toggle.TryActivate(scanner.Owner);
        UpdateScannedUser(scanner, patient, true);
    }

    private void StopAnalyzing(Entity<GehennaTraumaScannerComponent> scanner, EntityUid patient)
    {
        scanner.Comp.ScannedEntity = null;
        _toggle.TryDeactivate(scanner.Owner);
        UpdateScannedUser(scanner, patient, false);
    }

    private void PauseAnalyzing(Entity<GehennaTraumaScannerComponent> scanner, EntityUid patient)
    {
        if (!scanner.Comp.IsAnalyzerActive)
            return;

        UpdateScannedUser(scanner, patient, false);
        scanner.Comp.IsAnalyzerActive = false;
    }

    private void UpdateScannedUser(EntityUid scanner, EntityUid patient, bool scanMode)
    {
        if (!_ui.HasUi(scanner, GehennaTraumaScannerUiKey.Key))
            return;

        _ui.ServerSendUiMessage(scanner, GehennaTraumaScannerUiKey.Key, new GehennaTraumaScannerScannedUserMessage(GetState(patient, scanMode)));
    }

    private GehennaTraumaScannerUiState GetState(EntityUid patient, bool scanMode)
    {
        var name = Identity.Name(patient, EntityManager);
        var species = TryComp<HumanoidProfileComponent>(patient, out var humanoid)
            ? Loc.GetString(_prototypes.Index<SpeciesPrototype>(humanoid.Species).Name)
            : Loc.GetString("health-analyzer-window-entity-unknown-species-text");
        var state = TryComp<MobStateComponent>(patient, out var mobState) ? mobState.CurrentState : (Content.Shared.Mobs.MobState?) null;

        var bloodLevel = float.NaN;
        var bleeding = false;
        if (TryComp<BloodstreamComponent>(patient, out var bloodstream) &&
            _solution.ResolveSolution(patient, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out _))
        {
            bloodLevel = _bloodstream.GetBloodLevel(patient);
            bleeding = bloodstream.BleedAmount > 0;
        }

        return new GehennaTraumaScannerUiState(
            GetNetEntity(patient),
            name,
            species,
            state,
            bloodLevel,
            bleeding,
            scanMode,
            _trauma.GetScannerEntries(patient));
    }
}
