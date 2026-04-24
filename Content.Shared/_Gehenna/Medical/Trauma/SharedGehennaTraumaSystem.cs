using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Movement.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Gehenna.Medical.Trauma;

public sealed class SharedGehennaTraumaSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly ProtoId<DamageTypePrototype> Blunt = "Blunt";
    private static readonly ProtoId<DamageTypePrototype> Slash = "Slash";
    private static readonly ProtoId<DamageTypePrototype> Piercing = "Piercing";
    private static readonly ProtoId<DamageTypePrototype> Heat = "Heat";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamageableComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<GehennaTraumaComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<GehennaPainComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GehennaTraumaComponent>();
        while (query.MoveNext(out var uid, out var trauma))
        {
            var changed = false;
            for (var i = 0; i < trauma.Wounds.Count; i++)
            {
                var wound = trauma.Wounds[i];
                if (wound.State != GehennaWoundState.Open || _timing.CurTime < wound.CreatedAt + trauma.RotDelay)
                    continue;

                wound.State = GehennaWoundState.Rotting;
                changed = true;
            }

            if (!changed)
                continue;

            Dirty(uid, trauma);
            RefreshPain(uid, trauma);
        }
    }

    private void OnDamageChanged(Entity<DamageableComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta == null)
            return;

        var trauma = EnsureComp<GehennaTraumaComponent>(ent);
        EnsureComp<GehennaTargetingComponent>(ent);

        var zone = GetTargetZone(ent.Owner);
        var changed = false;

        foreach (var (damageType, amount) in args.DamageDelta.DamageDict)
        {
            if (amount <= FixedPoint2.Zero)
                continue;

            if (!TryGetTraumaType(damageType, args.Origin, out var traumaType))
                continue;

            ApplyWound((ent.Owner, trauma), zone, traumaType, damageType, amount);
            changed = true;
        }

        if (!changed)
            return;

        Dirty(ent.Owner, trauma);
        RefreshPain(ent.Owner, trauma);
    }

    private void OnGetVerbs(Entity<GehennaTraumaComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (args.User != args.Target || !args.CanInteract)
            return;

        var user = args.User;
        foreach (var zone in Enum.GetValues<GehennaBodyZone>())
        {
            var targetZone = zone;
            args.Verbs.Add(new Verb
            {
                Text = Loc.GetString($"gehenna-target-zone-{zone.ToString().ToLowerInvariant()}"),
                Category = VerbCategory.SelectType,
                Act = () => SetTargetZone(user, targetZone),
            });
        }
    }

    private void OnRefreshMovementSpeed(Entity<GehennaPainComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        var pain = ent.Comp.Pain.Float();
        if (pain <= 0)
            return;

        var modifier = Math.Clamp(1f - pain / 160f, 0.55f, 1f);
        args.ModifySpeed(modifier);
    }

    public bool TryBandage(Entity<GehennaTraumaComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        for (var i = 0; i < ent.Comp.Wounds.Count; i++)
        {
            var wound = ent.Comp.Wounds[i];
            if (wound.Type == GehennaTraumaType.Burn || wound.State != GehennaWoundState.Open)
                continue;

            wound.State = GehennaWoundState.Bandaged;
            wound.LastTreatedAt = _timing.CurTime;
            Dirty(ent.Owner, ent.Comp);
            RefreshPain(ent.Owner, ent.Comp);
            return true;
        }

        return false;
    }

    public bool TryOintment(Entity<GehennaTraumaComponent?> ent, int maxBurnDegree)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        for (var i = 0; i < ent.Comp.Wounds.Count; i++)
        {
            var wound = ent.Comp.Wounds[i];
            if (wound.Type != GehennaTraumaType.Burn || wound.BurnDegree > maxBurnDegree)
                continue;

            wound.State = GehennaWoundState.Bandaged;
            wound.LastTreatedAt = _timing.CurTime;
            wound.Severity = FixedPoint2.Max(FixedPoint2.Zero, wound.Severity - FixedPoint2.New(5));
            if (wound.Severity <= FixedPoint2.Zero)
                ent.Comp.Wounds.RemoveAt(i);

            Dirty(ent.Owner, ent.Comp);
            RefreshPain(ent.Owner, ent.Comp);
            return true;
        }

        return false;
    }

    public bool TrySuture(Entity<GehennaTraumaComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        for (var i = 0; i < ent.Comp.Wounds.Count; i++)
        {
            var wound = ent.Comp.Wounds[i];
            if (wound.Type == GehennaTraumaType.Burn)
                continue;

            ent.Comp.Wounds.RemoveAt(i);
            _damageable.TryChangeDamage(ent.Owner, -wound.Damage, true, false);
            Dirty(ent.Owner, ent.Comp);
            RefreshPain(ent.Owner, ent.Comp);
            return true;
        }

        return false;
    }

    public List<GehennaTraumaScannerEntry> GetScannerEntries(Entity<GehennaTraumaComponent?> ent)
    {
        var entries = new List<GehennaTraumaScannerEntry>();
        if (!Resolve(ent, ref ent.Comp, false))
            return entries;

        foreach (var wound in ent.Comp.Wounds)
        {
            entries.Add(new GehennaTraumaScannerEntry
            {
                Zone = wound.Zone,
                Type = wound.Type,
                State = wound.State,
                Severity = wound.Severity,
                BurnDegree = wound.BurnDegree,
                Treatment = GetTreatmentKey(wound),
            });
        }

        return entries;
    }

    public bool HasTreatableWound(Entity<GehennaTraumaComponent?> ent, GehennaTreatmentKind treatment, int maxBurnDegree)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        foreach (var wound in ent.Comp.Wounds)
        {
            if (treatment == GehennaTreatmentKind.Bandage &&
                wound.Type != GehennaTraumaType.Burn &&
                wound.State == GehennaWoundState.Open)
            {
                return true;
            }

            if (treatment == GehennaTreatmentKind.Suture && wound.Type != GehennaTraumaType.Burn)
                return true;

            if (treatment == GehennaTreatmentKind.Ointment &&
                wound.Type == GehennaTraumaType.Burn &&
                wound.BurnDegree <= maxBurnDegree)
            {
                return true;
            }
        }

        return false;
    }

    public void SetTargetZone(EntityUid uid, GehennaBodyZone zone)
    {
        var targeting = EnsureComp<GehennaTargetingComponent>(uid);
        targeting.TargetZone = zone;
        Dirty(uid, targeting);
    }

    private GehennaBodyZone GetTargetZone(EntityUid uid)
    {
        return TryComp<GehennaTargetingComponent>(uid, out var targeting)
            ? targeting.TargetZone
            : GehennaBodyZone.Torso;
    }

    private void ApplyWound(
        Entity<GehennaTraumaComponent> ent,
        GehennaBodyZone zone,
        GehennaTraumaType type,
        ProtoId<DamageTypePrototype> damageType,
        FixedPoint2 amount)
    {
        var wound = FindWound(ent.Comp, zone, type);
        if (wound == null)
        {
            wound = new GehennaWoundData
            {
                Zone = zone,
                Type = type,
                CreatedAt = _timing.CurTime,
                BurnDegree = type == GehennaTraumaType.Burn ? 1 : 0,
            };
            ent.Comp.Wounds.Add(wound);
        }
        else if (type == GehennaTraumaType.Burn)
        {
            wound.BurnDegree = Math.Min(4, wound.BurnDegree + 1);
        }

        wound.Severity += amount;
        wound.Damage.DamageDict[damageType] = wound.Damage.DamageDict.GetValueOrDefault(damageType) + amount;

        if (wound.State == GehennaWoundState.Bandaged)
            wound.State = GehennaWoundState.Open;
    }

    private static GehennaWoundData? FindWound(GehennaTraumaComponent comp, GehennaBodyZone zone, GehennaTraumaType type)
    {
        foreach (var wound in comp.Wounds)
        {
            if (wound.Zone == zone && wound.Type == type)
                return wound;
        }

        return null;
    }

    private bool TryGetTraumaType(ProtoId<DamageTypePrototype> damageType, EntityUid? origin, out GehennaTraumaType type)
    {
        if (damageType == Blunt)
        {
            type = GehennaTraumaType.Bruise;
            return true;
        }

        if (damageType == Slash)
        {
            type = GehennaTraumaType.Cut;
            return true;
        }

        if (damageType == Piercing)
        {
            type = IsLikelyGunshot(origin) ? GehennaTraumaType.Gunshot : GehennaTraumaType.Puncture;
            return true;
        }

        if (damageType == Heat)
        {
            type = GehennaTraumaType.Burn;
            return true;
        }

        type = default;
        return false;
    }

    private bool IsLikelyGunshot(EntityUid? origin)
    {
        return origin != null && HasComp<Content.Shared.Weapons.Ranged.Components.GunComponent>(origin.Value);
    }

    private void RefreshPain(EntityUid uid, GehennaTraumaComponent trauma)
    {
        var painValue = FixedPoint2.Zero;
        foreach (var wound in trauma.Wounds)
        {
            var multiplier = wound.State switch
            {
                GehennaWoundState.Open => 1.0f,
                GehennaWoundState.Rotting => 1.25f,
                GehennaWoundState.Septic => 1.5f,
                GehennaWoundState.Bandaged => 0.45f,
                _ => 0f,
            };

            if (wound.Type == GehennaTraumaType.Burn)
                multiplier += wound.BurnDegree * 0.15f;

            painValue += FixedPoint2.New(wound.Severity.Float() * multiplier);
        }

        if (painValue <= FixedPoint2.Zero)
        {
            if (HasComp<GehennaPainComponent>(uid))
            {
                RemComp<GehennaPainComponent>(uid);
                EntityManager.System<Content.Shared.Movement.Systems.MovementSpeedModifierSystem>().RefreshMovementSpeedModifiers(uid);
            }

            return;
        }

        var pain = EnsureComp<GehennaPainComponent>(uid);
        pain.Pain = painValue;
        Dirty(uid, pain);

        EntityManager.System<Content.Shared.Movement.Systems.MovementSpeedModifierSystem>().RefreshMovementSpeedModifiers(uid);
    }

    private static string GetTreatmentKey(GehennaWoundData wound)
    {
        if (wound.Type == GehennaTraumaType.Burn)
        {
            return wound.BurnDegree switch
            {
                <= 1 => "gehenna-trauma-treatment-ointment",
                2 => "gehenna-trauma-treatment-ointment-gauze",
                3 => "gehenna-trauma-treatment-dermazine-ointment-bandage",
                _ => "gehenna-trauma-treatment-cryo",
            };
        }

        return wound.State switch
        {
            GehennaWoundState.Open => "gehenna-trauma-treatment-bandage",
            GehennaWoundState.Bandaged => "gehenna-trauma-treatment-suture",
            GehennaWoundState.Rotting => "gehenna-trauma-treatment-clean-suture",
            GehennaWoundState.Septic => "gehenna-trauma-treatment-sepsis",
            _ => "gehenna-trauma-treatment-none",
        };
    }
}
