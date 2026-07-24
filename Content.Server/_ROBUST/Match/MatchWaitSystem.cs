using Robust.Shared.Timing;

namespace Content.Server._ROBUST.Match;

public sealed partial class MatchWaitSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MatchManagerSystem _match = default!;

    public override void Initialize()
    {
        base.Initialize();

        // SubscribeLocalEvent<MatchMapComponent, MatchStartedEvent>(OnMatchStart);
        // SubscribeLocalEvent<MatchMapComponent, MatchResetEvent>(OnMatchReset);
        // SubscribeLocalEvent<MatchMapComponent, MatchEndedEvent>(OnMatchEnd);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Get the current server time.
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<MatchWaitComponent>();

        // Loop over all entities.
        while (query.MoveNext(out var uid, out var comp))
        {
            // Skip this entity if it should not be updated yet.
            if (comp.StopWaitingTime == null || comp.StopWaitingTime > curTime)
                continue;

            comp.StopWaitingTime = null;

            _match.EndCurrentPhase(uid);
        }
    }
}

public sealed partial class Wait : IPhase
{
    [DataField(required: true)]
    public TimeSpan WaitTime;

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();

        entMan.EnsureComponent<MatchWaitComponent>(match).StopWaitingTime = matchSystem._timing.CurTime + WaitTime;
    }
}
