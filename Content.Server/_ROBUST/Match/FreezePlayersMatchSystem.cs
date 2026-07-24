using Content.Shared._ROBUST.Match;
using Content.Shared.CombatMode.Pacification;
using Robust.Shared.Timing;

namespace Content.Server._ROBUST.Match;

public sealed partial class FreezePlayersMatchSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MatchManagerSystem _match = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Get the current server time.
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<FreezePlayersMatchComponent>();

        // Loop over all entities.
        while (query.MoveNext(out var uid, out var comp))
        {
            // Skip this entity if it should not be updated yet.
            if (comp.UnfreezeTime == null || comp.UnfreezeTime > curTime)
                continue;

            comp.UnfreezeTime = null;

            foreach (var player in _match.GetPlayers(uid))
            {
                if (player.AttachedEntity == null)
                    throw new Exception("Tried to freeze player who has no entity");

                RemComp<FreezeComponent>(player.AttachedEntity.Value);
                RemComp<PacifiedComponent>(player.AttachedEntity.Value);
            }

            _match.EndCurrentPhase(uid);
        }
    }
}

public sealed partial class FreezePlayers : IPhase
{
    [DataField(required: true)]
    public TimeSpan FreezeTime;

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();

        entMan.EnsureComponent<FreezePlayersMatchComponent>(match).UnfreezeTime = matchSystem._timing.CurTime + FreezeTime;

        foreach (var player in matchSystem.GetPlayers(match))
        {
            if (player.AttachedEntity == null)
                throw new Exception("Tried to freeze player who has no entity");

            entMan.EnsureComponent<FreezeComponent>(player.AttachedEntity.Value);
            var pacified = entMan.EnsureComponent<PacifiedComponent>(player.AttachedEntity.Value); // todo move this to another system
            pacified.DisallowAllCombat = true;
        }
    }
}

