using System.Linq;
using Content.Server._ROBUST.DebugSystem;
using Content.Shared._ROBUST.Match;
using Content.Shared.Ghost;
using Content.Shared.Ghost.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._ROBUST.Match;

public sealed partial class RoundManagerSystem : EntitySystem
{
     [Dependency] private ActorSystem _actor = default!;
     [Dependency] private MatchManagerSystem _match = default!;
     [Dependency] private INetManager _net = default!;
     [Dependency] private ILogManager _log = default!;
     [Dependency] private MobStateSystem _mobState = default!;
     [Dependency] private IGameTiming _timing = default!;
     [Dependency] private ISharedPlayerManager _player = default!;
     [Dependency] private MatchTeamsSystem _team = default!;
     [Dependency] private DebugMatch _debug = default!;

     private ISawmill _sawmill = default!;

     public override void Initialize()
     {
         base.Initialize();

         _sawmill = _log.GetSawmill("RoundManager");

         // SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);

         // SubscribeLocalEvent<RoundComponent, RoundStartEvent>(OnRoundStart, after: [ typeof(MatchMapSystem) ]);
         _net.RegisterNetMessage<CurrentScoreMessage>();
         _net.RegisterNetMessage<HideScoreboard>();
         _net.RegisterNetMessage<SetTimerScoreMessage>();

     }

     // todo: do check ensure you aren't in a weird state like in the middle of phase that you shouldn't be.
     // private void OnMobStateChanged(MobStateChangedEvent args)
     // {
     //     if (args.NewMobState == MobState.Alive)
     //         return;
     //
     //     var session = _actor.GetSession(args.Target);
     //
     //     if (session == null)
     //         return;
     //
     //     var query = EntityQueryEnumerator<RoundComponent, MatchComponent>();
     //     while (query.MoveNext(out var uid, out var roundComp, out var matchComp))
     //     {
     //         if (!matchComp.Players.Contains(session))
     //             continue;
     //
     //         // todo: do more robust checks ensure both are players in game etc...
     //         //  if null throw error etc...
     //         // get other player, give them the point
     //         var orginsession = _actor.GetSession(args.Origin);
     //         if (args.Origin != null && orginsession != null)
     //             roundComp.PlayerToRoundsWon[orginsession]++;
     //
     //         foreach (var player in roundComp.PlayerToRoundsWon)
     //         {
     //             _sawmill.Info($"{player.Key.UserId.UserId} -> {player.Value}");
     //         }
     //
     //         MakeNewRound((uid, roundComp));
     //     }
     // }

     // Make a new round
     public void MakeNewRound(Entity<RoundComponent> ent)
     {
         _debug.Serialize(ent, "NewRound");

         ent.Comp.RoundNumber++;

         // Send round status here.

         foreach (var player in _match.GetPlayers(ent))
         {
             var matchStatus = new CurrentScoreMessage
             {
                 RoundNumber = ent.Comp.RoundNumber,
                 FirstToPoints = ent.Comp.FirstToPoints,
                 Scores = ent.Comp.TeamToRoundsWon,
                 YourTeam = _team.GetTeamOfPlayer(ent, player),
             };

             player.Channel.SendMessage(matchStatus);
         }

         // TODO: Add assert that the round number is never greater than max rounds
         if (ent.Comp.FirstToPoints == ent.Comp.TeamToRoundsWon.Values.Max())
         {
             _match.StartPhase(ent.Owner, MatchPhaseSection.End);
             return;
         }

         _match.StartPhase(ent.Owner, MatchPhaseSection.RoundEnd);
     }

     public bool IsRoundGameplayOngoing(EntityUid uid)
     {
         return Comp<RoundComponent>(uid).CurrentRoundStartTime != null;
     }

     private bool CheckIfTimeout(Entity<RoundComponent, MatchComponent> match)
     {
         if (_timing.CurTime < match.Comp1.CurrentRoundStartTime + match.Comp1.MaxRoundLength)
             return false;

         MakeNewRound((match.Owner, match.Comp1));

         return true;
     }

     private bool CheckHasWinner(Entity<RoundComponent, MatchComponent> match)
     {
         var teams = Comp<MatchTeamComponent>(match).Teams;

         // If one team has at least 1 alive player, and all other teams are dead. They are the winner.

         // Check if all players are dead.

         var aliveTeams = 0;
         var lastAliveTeamName = "";

         foreach (var team in teams)
         {
             var alivePlayer = false;

             foreach (var player in team.Value)
             {
                 if (player.AttachedEntity == null)
                     continue;

                 if (HasComp<GhostComponent>(player.AttachedEntity))
                     continue;

                 if (_mobState.IsIncapacitated(player.AttachedEntity.Value))
                     continue;

                 alivePlayer = true;
             }

             if (alivePlayer)
             {
                 aliveTeams++;
                 lastAliveTeamName = team.Key;
             }
         }

         if (aliveTeams == 0)
         {
             MakeNewRound((match.Owner, match.Comp1));
         }

         // only one alive team, make them the winner
         if (aliveTeams == 1)
         {
             // todo: this needs work its kinda sus.
             //  we know that there is only one alive team so we can just use lastAliveTeamName
             if (lastAliveTeamName == "")
                 throw new Exception();

             // todo: this should be a function call probably
             match.Comp1.TeamToRoundsWon[lastAliveTeamName]++;

             MakeNewRound((match.Owner, match.Comp1));

             return true;
         }

         return false;
     }

     public override void Update(float frameTime)
     {
         var query = EntityQueryEnumerator<RoundComponent, MatchComponent>();
         while (query.MoveNext(out var uid, out var roundComp, out var matchComp))
         {
             Entity<RoundComponent, MatchComponent> match = (uid, roundComp, matchComp);

             // The round isn't active
             if (!IsRoundGameplayOngoing(uid))
                 continue;

             if (CheckHasWinner(match))
                 continue;

             if (CheckIfTimeout(match))
                 continue;
         }
     }
}

// todo: reduce this sus copy paste
public sealed partial class SetupScores : IPhase
{
    private MatchTeamsSystem _team = default!;
    private MatchManagerSystem _match = default!;


    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        _team = entMan.System<MatchTeamsSystem>();
        _match = entMan.System<MatchManagerSystem>();

        var roundComp = entMan.EnsureComponent<RoundComponent>(match);

        entMan.System<DebugMatch>().Serialize(match, "SetupScores");

        // Send round status here.

        foreach (var teamName in _team.GetTeams(match))
        {
            roundComp.TeamToRoundsWon[teamName] = 0;
        }

        foreach (var player in _match.GetPlayers(match))
        {
            var matchStatus = new CurrentScoreMessage
            {
                RoundNumber = roundComp.RoundNumber,
                FirstToPoints = roundComp.FirstToPoints,
                Scores = roundComp.TeamToRoundsWon,
                YourTeam = _team.GetTeamOfPlayer(match, player),
            };

            matchStatus.Scores = roundComp.TeamToRoundsWon;

            player.Channel.SendMessage(matchStatus);
        }

        EndPhase(match, entMan);
    }
}

public sealed partial class HideScores : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSys = entMan.System<MatchManagerSystem>();

        foreach (var player in matchSys.GetPlayers(match))
        {
            player.Channel.SendMessage(new HideScoreboard());
        }

        EndPhase(match, entMan);
    }
}

public sealed partial class RoundStartGameplay : IPhase
{
    [Dependency] private IGameTiming _timing;

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        IoCManager.InjectDependencies(this);

        var matchSys = entMan.System<MatchManagerSystem>();

        var roundComp = entMan.EnsureComponent<RoundComponent>(match);
        roundComp.CurrentRoundStartTime = _timing.CurTime;

        foreach (var player in matchSys.GetPlayers(match))
        {
            player.Channel.SendMessage(new SetTimerScoreMessage { TimerLength = roundComp.MaxRoundLength });
        }

        EndPhase(match, entMan);
    }
}

public sealed partial class RoundEndGameplay : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSys = entMan.System<MatchManagerSystem>();

        var roundComp = entMan.EnsureComponent<RoundComponent>(match);
        roundComp.CurrentRoundStartTime = null;

        foreach (var player in matchSys.GetPlayers(match))
        {
            player.Channel.SendMessage(new SetTimerScoreMessage { TimerLength = TimeSpan.FromSeconds(0) });
        }

        EndPhase(match, entMan);
    }
}
