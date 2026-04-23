using System.Numerics;
using System.Collections.Generic;
using Content.Server.Storage.EntitySystems;
using Content.Shared._Gehenna.Prison.Chest;
using Content.Shared.Destructible;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server._Gehenna.Prison.Chest;

public sealed class PrisonChestStashSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly StorageSystem _storage = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private readonly Dictionary<EntityUid, EntityUid> _stashEntities = new();
    private readonly Dictionary<EntityUid, EntityUid> _stashOwners = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrisonChestStashComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PrisonChestStashComponent, DestructionEventArgs>(OnDestroyed);
        SubscribeLocalEvent<PrisonChestStashComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<PrisonChestStashInnerComponent, BoundUIClosedEvent>(OnStashUiClosed);
        SubscribeLocalEvent<PrisonChestStashComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnMapInit(Entity<PrisonChestStashComponent> ent, ref MapInitEvent args)
    {
        EnsureStash(ent);
    }

    private void OnDestroyed(Entity<PrisonChestStashComponent> ent, ref DestructionEventArgs args)
    {
        CleanupStash(ent, spillContents: true);
    }

    private void OnTerminating(Entity<PrisonChestStashComponent> ent, ref EntityTerminatingEvent args)
    {
        CleanupStash(ent, spillContents: true);
    }

    private void OnGetVerbs(Entity<PrisonChestStashComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var isRevealed = ent.Comp.StashRevealed;
        var user = args.User;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString(isRevealed
                ? "prison-chest-stash-verb-close"
                : "prison-chest-stash-verb-open"),
            Icon = new SpriteSpecifier.Texture(
                new ResPath("/Textures/Interface/VerbIcons/open.svg.192dpi.png")),
            Act = () => ToggleStash(ent, user),
            Priority = 2,
        });
    }

    private void OnStashUiClosed(Entity<PrisonChestStashInnerComponent> ent, ref BoundUIClosedEvent args)
    {
        if (!_stashOwners.TryGetValue(ent.Owner, out var chestUid))
            return;

        if (_ui.IsUiOpen(ent.Owner, args.UiKey))
            return;

        if (!TryComp<PrisonChestStashComponent>(chestUid, out var chestComp) || !chestComp.StashRevealed)
            return;

        chestComp.StashRevealed = false;
        _appearance.SetData(chestUid, PrisonStashVisuals.StashRevealed, false);
        Dirty(chestUid, chestComp);
    }

    private EntityUid? EnsureStash(Entity<PrisonChestStashComponent> ent)
    {
        if (_stashEntities.TryGetValue(ent.Owner, out var existing) && Exists(existing))
            return existing;

        var stashEnt = Spawn(ent.Comp.StashProto, Transform(ent.Owner).Coordinates);
        _transform.SetParent(stashEnt, ent.Owner);
        _transform.SetLocalPosition(stashEnt, Vector2.Zero);

        _stashEntities[ent.Owner] = stashEnt;
        _stashOwners[stashEnt] = ent.Owner;
        return stashEnt;
    }

    private void ToggleStash(Entity<PrisonChestStashComponent> ent, EntityUid user)
    {
        if (ent.Comp.StashRevealed)
            CloseStash(ent, user);
        else
            OpenStash(ent, user);
    }

    private void OpenStash(Entity<PrisonChestStashComponent> ent, EntityUid user)
    {
        if (EnsureStash(ent) is not { } stashEnt)
            return;

        ent.Comp.StashRevealed = true;
        _appearance.SetData(ent, PrisonStashVisuals.StashRevealed, true);

        _storage.OpenStorageUI(stashEnt, user, silent: true);
        Dirty(ent);
    }

    private void CloseStash(Entity<PrisonChestStashComponent> ent, EntityUid user)
    {
        ent.Comp.StashRevealed = false;
        _appearance.SetData(ent, PrisonStashVisuals.StashRevealed, false);

        if (_stashEntities.TryGetValue(ent.Owner, out var stashEnt) && Exists(stashEnt))
            _ui.CloseUi(stashEnt, StorageComponent.StorageUiKey.Key, user);

        Dirty(ent);
    }

    private void CleanupStash(Entity<PrisonChestStashComponent> ent, bool spillContents)
    {
        if (!_stashEntities.Remove(ent.Owner, out var stashEnt))
            return;

        _stashOwners.Remove(stashEnt);

        if (!Exists(stashEnt))
            return;

        if (spillContents && TryComp<StorageComponent>(stashEnt, out var stashStorage))
        {
            var destination = _transform.GetMoverCoordinates(stashEnt);
            _transform.AttachToGridOrMap(stashEnt);
            _container.EmptyContainer(stashStorage.Container, force: true, destination: destination);
        }

        if (Exists(stashEnt))
            QueueDel(stashEnt);
    }
}
