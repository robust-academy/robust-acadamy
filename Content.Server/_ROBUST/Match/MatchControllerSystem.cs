using System.Linq;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared._ROBUST.Match;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Ghost.Components;
using Content.Shared.Verbs;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._ROBUST.Match;

public sealed partial class MatchControllerSystem : EntitySystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private GameTicker _ticker = default!;
    [Dependency] private MatchManagerSystem _match = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private IChatManager _chat = default!;

    // the player they want to start a match with -> player wanting to start a match
    private Dictionary<ICommonSession, (TimeSpan, ICommonSession)> WantingToStartMatchToTarget = new();

    public override void Initialize()
    {
        base.Initialize();

        // SubscribeLocalEvent<GetVerbsEvent<Verb>>(GetVerbs);

        _net.RegisterNetMessage<ConfirmMatchStart>(OnConfirmMatchStart);

        _net.Disconnect += OnDisconnect;

        SubscribeLocalEvent<RoundRestartCleanupEvent>(Reset);
        SubscribeLocalEvent<MatchSelectionComponent, MatchAttemptStart>(OnMatchAttemptStart);
    }

    // todo: move this
    private void OnMatchAttemptStart(Entity<MatchSelectionComponent> ent, ref MatchAttemptStart args)
    {
        var ents = _lookup.GetEntitiesInRange(Transform(ent).Coordinates, 2);

        List<ICommonSession> players = new();

        foreach (var entity in ents)
        {
            if (!TryComp<ActorComponent>(entity, out var actor) || HasComp<GhostComponent>(entity))
                continue;

            players.Add(actor.PlayerSession);
        }

        var proto = ProtoMan.Index(args.MatchProtoId);

        // todo: use the onMatchStart thing for this.
        if (args.MatchProtoId == "1v1Match" && players.Count >= 2)
        {
            var p = players.Shuffle().ToList();
            List<ICommonSession> p2 = [p.Pop(), p.Pop()];

            _match.StartMatch(proto.MatchEntProtoId, p2, args.MatchSettings);
            return;
        }

        if (args.MatchProtoId == "1v1Uplink" && players.Count >= 2)
        {
            var p = players.Shuffle().ToList();
            List<ICommonSession> p2 = [p.Pop(), p.Pop()];

            _match.StartMatch(proto.MatchEntProtoId, p2, args.MatchSettings);
            return;
        }

        if (args.MatchProtoId == "1v1v1Match" && players.Count >= 3)
        {
            var p = players.Shuffle().ToList();
            List<ICommonSession> p2 = [p.Pop(), p.Pop(), p.Pop()];

            _match.StartMatch(proto.MatchEntProtoId, p2, args.MatchSettings);
            return;
        }

        if (args.MatchProtoId == "2v2Match" && players.Count >= 4)
        {
            var p = players.Shuffle().ToList();
            List<ICommonSession> p2 = [p.Pop(), p.Pop(), p.Pop(), p.Pop()];

            _match.StartMatch(proto.MatchEntProtoId, p2, args.MatchSettings);
            return;
        }

        if (args.MatchProtoId == "3v3Match" && players.Count >= 6)
        {
            var p = players.Shuffle().ToList();
            List<ICommonSession> p2 = [p.Pop(), p.Pop(), p.Pop(), p.Pop(), p.Pop(), p.Pop()];

            _match.StartMatch(proto.MatchEntProtoId, p2, args.MatchSettings);
            return;
        }

        _chat.DispatchServerAnnouncement("Could not start match!");
    }

    private void Reset(RoundRestartCleanupEvent ev)
    {
        WantingToStartMatchToTarget = new Dictionary<ICommonSession, (TimeSpan, ICommonSession)>();
    }

    private void OnDisconnect(object? sender, NetDisconnectedArgs e)
    {
        WantingToStartMatchToTarget = new Dictionary<ICommonSession, (TimeSpan, ICommonSession)>();
    }

    private void OnConfirmMatchStart(ConfirmMatchStart message)
    {
        // throw new NotImplementedException();

        var user = _player.GetSessionByChannel(message.MsgChannel);

        if (!WantingToStartMatchToTarget.ContainsKey(user))
            return;


        _match.StartMatch("TestMatch", [ user,  WantingToStartMatchToTarget[user].Item2 ]);
    }

    private void GetVerbs(GetVerbsEvent<Verb> args)
    {

        if (args.Target == args.User)
            return;

        if (!TryComp<ActorComponent>(args.User, out var userActor))
            return;

        if (!TryComp<ActorComponent>(args.Target, out var targetActor))
            return;

        // todo: fix this and just make it so ghosts can't start matches or something
        if (HasComp<GhostComponent>(targetActor.PlayerSession.AttachedEntity) || HasComp<GhostComponent>(targetActor.PlayerSession.AttachedEntity))
            return;

        var allPlayersInMatch = _match.GetAllPlayersInMatches();

        if (allPlayersInMatch.Contains(userActor.PlayerSession) || allPlayersInMatch.Contains(targetActor.PlayerSession))
            return;

        // for now just only allow one outgoing request / incoming request at a time. Makes the logic simple and
        // hopefully bug free.

        if (WantingToStartMatchToTarget.ContainsKey(userActor.PlayerSession) ||
            WantingToStartMatchToTarget.Values.Select(x => x.Item2).Contains(userActor.PlayerSession))
            return;

        if (WantingToStartMatchToTarget.ContainsKey(targetActor.PlayerSession) ||
            WantingToStartMatchToTarget.Values.Select(x => x.Item2).Contains(targetActor.PlayerSession))
            return;

        Verb match1 = new()
        {
            Text = "Request a match",
            Act = () =>
            {
                // List<ICommonSession> players = [ userActor.PlayerSession, targetActor.PlayerSession ];

                SendMatchRequest(targetActor.PlayerSession, userActor.PlayerSession);
            }
        };
        args.Verbs.Add(match1);

        var allPlayers = _player.GetAllPlayerData().ToList();

        var allPlayersInMatc = _match.GetAllPlayersInMatches();

        var freePlayers = new List<ICommonSession>();
        foreach (var player in allPlayers)
        {
            var session = _player.GetSessionById(player.UserId);

            if (allPlayersInMatc.Contains(session))
                continue;

            if (session.AttachedEntity == null)
                continue;

            if (!HasComp<ActorComponent>(session.AttachedEntity))
                continue;

            if (HasComp<GhostComponent>(session.AttachedEntity))
                continue;

            freePlayers.Add(session);
        }

        if (freePlayers.Count < 4)
            return;

        List<ICommonSession> twovtwoplayers = [freePlayers.Pop(), freePlayers.Pop(), freePlayers.Pop(), freePlayers.Pop()];

        Verb match2 = new()
        {
            Text = "Start 2v2 match",
            Act = () =>
            {
                _match.StartMatch("TwoVsTwoMatch", twovtwoplayers);
            }
        };
        args.Verbs.Add(match2);
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
    //     // todo: fix this and just make it so ghosts can't start matches or something
    //     if (HasComp<GhostComponent>(targetActor.PlayerSession.AttachedEntity) || HasComp<GhostComponent>(targetActor.PlayerSession.AttachedEntity))
    //         return;
    //
    //     var allPlayers = _match.GetAllPlayersInMatches();
    //
    //     if (allPlayers.Contains(userActor.PlayerSession) || allPlayers.Contains(targetActor.PlayerSession))
    //         return;
    //
    //     // for now just only allow one outgoing request / incoming request at a time. Makes the logic simple and
    //     // hopefully bug free.
    //
    //     if (WantingToStartMatchToTarget.ContainsKey(userActor.PlayerSession) ||
    //         WantingToStartMatchToTarget.Values.Select(x => x.Item2).Contains(userActor.PlayerSession))
    //         return;
    //
    //     if (WantingToStartMatchToTarget.ContainsKey(targetActor.PlayerSession) ||
    //         WantingToStartMatchToTarget.Values.Select(x => x.Item2).Contains(targetActor.PlayerSession))
    //         return;
    //
    //     Verb match = new()
    //     {
    //         Text = "Request a match",
    //         Act = () =>
    //         {
    //             // List<ICommonSession> players = [ userActor.PlayerSession, targetActor.PlayerSession ];
    //
    //             SendMatchRequest(targetActor.PlayerSession, userActor.PlayerSession);
    //         }
    //     };
    //     args.Verbs.Add(match);
    // }

    private void SendMatchRequest(ICommonSession target, ICommonSession starter)
    {
        WantingToStartMatchToTarget.Add(target, (_timing.CurTime, starter));

        var message = new RequestMatchStart();
        message.Username = starter.Name;
        RaiseNetworkEvent(message, target.Channel);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        List<ICommonSession> ToDelete = new();

        foreach (var pair in WantingToStartMatchToTarget)
        {
            var startedTime = pair.Value.Item1;
            if (_timing.CurTime < startedTime + TimeSpan.FromSeconds(10))
                continue;

            ToDelete.Add(pair.Key);
        }

        foreach (var key in ToDelete)
        {
            WantingToStartMatchToTarget.Remove(key);
        }

    }
}
