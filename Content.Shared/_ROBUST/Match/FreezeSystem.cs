using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;

namespace Content.Shared._ROBUST.Match;

public sealed partial class FreezeSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FreezeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FreezeComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<FreezeComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
    }

    private void OnMapInit(Entity<FreezeComponent> ent, ref MapInitEvent args)
    {
        _blocker.UpdateCanMove(ent);
    }

    private void OnUpdateCanMove(Entity<FreezeComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    private void OnComponentShutdown(Entity<FreezeComponent> ent, ref ComponentShutdown args)
    {
        _blocker.UpdateCanMove(ent);
    }
}
