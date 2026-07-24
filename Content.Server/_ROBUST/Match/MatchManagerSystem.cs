using System.Linq;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Shared._ROBUST.Match;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Server._ROBUST.Match;

public sealed partial class MatchManagerSystem : EntitySystem
{
    [Dependency] public IPlayerManager _player = default!;
    [Dependency] public IGameTiming _timing = default!;
    [Dependency] public INetManager _net = default!;
    [Dependency] public GameTicker _ticker = default!;
    [Dependency] private MatchPlayerBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();

        // SubscribeLocalEvent<GetVerbsEvent<Verb>>(GetVerbs);
        SubscribeLocalEvent<MatchComponent, PhaseEndedEvent>(OnPhaseEnd);

        SubscribeLocalEvent<IsRoleAllowedEvent>(OnIsRoleAllowed);

        _net.Disconnect += OnDisconnect;
    }

    private void OnIsRoleAllowed(ref IsRoleAllowedEvent ev)
    {
        ev.Cancelled = true;
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs e)
    {
        var query = EntityQueryEnumerator<MatchComponent>();
        while (query.MoveNext(out var uid, out var matchComp))
        {
            // put all players as uuid
            var players = GetPlayers(uid);

            // cast player to uuid
            if (!players.Select(x => x.UserId).ToList().Contains(e.Channel.UserId))
                continue;

            DeleteMatchAndRespawnPlayers(uid);
        }
    }

    public void DeleteMatchAndRespawnPlayers(EntityUid match)
    {
        foreach (var player in GetPlayers(match))
        {
            _ticker.Respawn(player);
        }

        // todo: use events instead
        _body.DeleteAllBodies(match);

        Del(match);
    }

    public EntityUid? GetMatchFromPlayer(ICommonSession player)
    {
        var query = EntityQueryEnumerator<MatchComponent>();
        while (query.MoveNext(out var uid, out var matchComp))
        {
            // put all players as uuid
            var players = GetPlayers(uid);

            // cast player to uuid
            if (!players.Contains(player))
                continue;

            return uid;
        }

        return null;
    }

    // private void GetVerbs(GetVerbsEvent<Verb> args)
    // {
    //     if (args.Target == args.User)
    //         return;
    //
    //     if (!TryComp<ActorComponent>(args.User, out var userActor))
    //         return;
    //
    //     if (!TryComp<ActorComponent>(args.Target, out var targetActor))
    //         return;
    //
    //     var allPlayers = GetAllPlayersInMatches();
    //
    //     if (allPlayers.Contains(userActor.PlayerSession) || allPlayers.Contains(targetActor.PlayerSession))
    //         return;
    //
    //     Verb match = new()
    //     {
    //         Text = "START MATCH",
    //         Act = () =>
    //         {
    //             List<ICommonSession> players = [ userActor.PlayerSession, targetActor.PlayerSession ];
    //
    //             StartMatch("TestMatch", players);
    //         }
    //     };
    //     args.Verbs.Add(match);
    // }

    public void StartMatch(EntProtoId matchProto, List<ICommonSession> players, MatchSettings? matchSettings = null)
    {
        var matchEntity = Spawn(matchProto);
        var matchComp = Comp<MatchComponent>(matchEntity);

        matchComp.Players = players;

        // todo: Clients can send whatever they want here. Ensure that its sanitized at some point...
        //  for now, the systems themselves check the sanitization
        var matchStartEvent = new MatchStartedEvent(matchSettings);

        RaiseLocalEvent(matchEntity, ref matchStartEvent);

        StartPhase((matchEntity, matchComp), MatchPhaseSection.Start);
    }

    public void StartPhase(Entity<MatchComponent?> match, MatchPhaseSection matchPhase)
    {
        if (!Resolve(match.Owner, ref match.Comp, true))
            return;

        if (match.Comp.CurrentPhaseType != null)
            throw new Exception("Tried to start match phase but the current phase was not null.");

        match.Comp.CurrentPhaseType = matchPhase;

        if (!match.Comp.MatchPhases.TryGetValue(matchPhase, out var phases))
            throw new Exception("Tried to start match phase but could not find phase in match.");

        if (phases.Count == 0)
            throw new Exception("Tried to start match phase but there were no phases in the section.");

        // Run the first phase
        phases[0].RunPhase(match, EntityManager);
    }

    // todo fix me the name is the same
    private void EndCurrentPhase(Entity<MatchComponent?> match)
    {
        if (!Resolve(match.Owner, ref match.Comp, true))
            return;

        match.Comp.CurrentPhaseType = null;
        match.Comp.CurrentPhaseNumber = 0;
    }

    private void OnPhaseEnd(Entity<MatchComponent> match, ref PhaseEndedEvent phaseEndedEvent)
    {
        if (match.Comp.CurrentPhaseType == null)
            throw new Exception("Tried to end phase, but current phase was null.");

        var phaseSection = match.Comp.CurrentPhaseType;
        var phaseNumber = match.Comp.CurrentPhaseNumber;

        var currentPhase = match.Comp.MatchPhases[phaseSection][phaseNumber];

        // if (currentPhase.NextPhase is not MatchPhaseSection)
        //     throw new Exception("Phase is the incorrect type");

        var nextPhase = (MatchPhaseSection) currentPhase.NextPhase;

        if (nextPhase == MatchPhaseSection.S_EndCurrentPhase)
        {
            EndCurrentPhase(match.AsNullable());
            return;
        }

        if (nextPhase != MatchPhaseSection.S_ContinueCurrentPhase)
        {
            EndCurrentPhase(match.AsNullable());
            StartPhase(match.AsNullable(), nextPhase);
            return;
        }

        match.Comp.CurrentPhaseNumber++;

        if (match.Comp.CurrentPhaseNumber >= match.Comp.MatchPhases[phaseSection].Count)
            throw new Exception("Current phase number is greater than the total number of phases.");

        match.Comp.MatchPhases[phaseSection][match.Comp.CurrentPhaseNumber].RunPhase(match, EntityManager);
    }

    // todo fix this, the name is the same as the other one
    public void EndCurrentPhase(EntityUid match)
    {
        var evnt = new PhaseEndedEvent();
        RaiseLocalEvent(match, ref evnt);
    }

    public EntityUid GetGrid(EntityUid match)
    {
        // todo: get rid of magic string
        var grid = Comp<MatchMapComponent>(match).Grids["defaultMap"];

        return grid;
    }

    public EntityUid GetMap(EntityUid match)
    {
        // todo: get rid of magic string
        var map = Comp<MatchMapComponent>(match).Maps["defaultMap"];

        return map;
    }

    public List<ICommonSession> GetPlayers(EntityUid match)
    {
        var players = Comp<MatchComponent>(match).Players;

        return players;
    }

    public List<ICommonSession> GetAllPlayersInMatches()
    {
        var players = new List<ICommonSession>();

        var query = EntityQueryEnumerator<MatchComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            players.AddRange(GetPlayers(uid));
        }

        return players;
    }
}

[ByRefEvent]
public record struct MatchStartedEvent(MatchSettings? MatchSettings)
{
    public MatchSettings? MatchSettings = MatchSettings;
};

public sealed partial class DeleteMatch : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        entMan.DeleteEntity(match);
    }
}
