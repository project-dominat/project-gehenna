using Content.Shared._Gehenna.Medical.Trauma;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Gehenna.Medical.Trauma;

public sealed class GehennaTraumaTreatmentSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGehennaTraumaSystem _trauma = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStackSystem _stacks = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GehennaTraumaTreatmentComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<GehennaTraumaTreatmentComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<GehennaTraumaTreatmentComponent, GehennaTraumaDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<GehennaTraumaTreatmentComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        if (TryStartTreatment(ent, args.Target.Value, args.User))
            args.Handled = true;
    }

    private void OnUseInHand(Entity<GehennaTraumaTreatmentComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (TryStartTreatment(ent, args.User, args.User))
            args.Handled = true;
    }

    private void OnDoAfter(Entity<GehennaTraumaTreatmentComponent> ent, ref GehennaTraumaDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target == null)
            return;

        if (!TryComp<GehennaTraumaComponent>(args.Target, out var trauma))
            return;

        var treated = ent.Comp.Treatment switch
        {
            GehennaTreatmentKind.Bandage => _trauma.TryBandage((args.Target.Value, trauma)),
            GehennaTreatmentKind.Suture => _trauma.TrySuture((args.Target.Value, trauma)),
            GehennaTreatmentKind.Ointment => _trauma.TryOintment((args.Target.Value, trauma), ent.Comp.MaxBurnDegree, ent.Comp.BurnSeverityHealing),
            GehennaTreatmentKind.Tourniquet => _trauma.TryTourniquet((args.Target.Value, trauma)),
            GehennaTreatmentKind.Bloodpack => TryUseBloodpack(ent, args.Target.Value),
            _ => false,
        };

        if (!treated)
            return;

        UseCharge(ent.Owner);
        _audio.PlayPvs(ent.Comp.EndSound, args.Target.Value);
        _popup.PopupEntity(Loc.GetString("gehenna-trauma-treatment-finished", ("target", Identity.Entity(args.Target.Value, EntityManager))), args.Target.Value);
        args.Handled = true;
    }

    private bool TryStartTreatment(Entity<GehennaTraumaTreatmentComponent> ent, EntityUid target, EntityUid user)
    {
        if (!CanTreat(ent, target, user))
            return false;

        _audio.PlayPvs(ent.Comp.BeginSound, target);

        var doAfterArgs = new DoAfterArgs(EntityManager, user, ent.Comp.Delay, new GehennaTraumaDoAfterEvent(), ent, target: target, used: ent)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnDamage = true,
        };

        return _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private bool CanTreat(Entity<GehennaTraumaTreatmentComponent> ent, EntityUid target, EntityUid user)
    {
        if (TryComp<StackComponent>(ent, out var stack) && stack.Count <= 0)
            return false;

        if (ent.Comp.Treatment == GehennaTreatmentKind.Bloodpack)
            return CanUseBloodpack(target);

        if (!TryComp<GehennaTraumaComponent>(target, out var trauma))
            return false;

        return _trauma.HasTreatableWound((target, trauma), ent.Comp.Treatment, ent.Comp.MaxBurnDegree);
    }

    private bool CanUseBloodpack(EntityUid target)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return false;

        return _bloodstream.GetBloodLevel((target, bloodstream)) < 1f;
    }

    private bool TryUseBloodpack(Entity<GehennaTraumaTreatmentComponent> ent, EntityUid target)
    {
        if (!TryComp<BloodstreamComponent>(target, out var bloodstream))
            return false;

        var changedBlood = _bloodstream.TryModifyBloodLevel((target, bloodstream), ent.Comp.BloodRestoreAmount);
        var changedDamage = _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = { ["Bloodloss"] = -0.5 } }, true, false);
        return changedBlood || changedDamage;
    }

    private void UseCharge(EntityUid uid)
    {
        if (TryComp<StackComponent>(uid, out var stack))
        {
            _stacks.ReduceCount((uid, stack), 1);
            return;
        }

        QueueDel(uid);
    }
}
