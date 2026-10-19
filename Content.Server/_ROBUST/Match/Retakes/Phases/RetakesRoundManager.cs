using System.Linq;
using Content.Server._ROBUST.DebugSystem;
using Content.Server.Nuke;
using Content.Shared._ROBUST.Match;
using Content.Shared.Ghost;
using Content.Shared.Ghost.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._ROBUST.Match.Retakes.Phases;

public sealed partial class RetakesRoundManager : EntitySystem
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

     private bool CheckHasWinner(Entity<RetakesRoundComponent, MatchComponent> match)
     {
        // Check if the nuke exists (if it does not, the round is over)
        // Check if all crew are dead (if they are, the round is over)
        var matchMap = _match.GetMap(match);

        var nukes = EntityQueryEnumerator<NukeComponent>();

        var numNukes = 0;

        foreach (var nuke in nukes)
        {
            var nukeMapUid = Comp<TransformComponent>(nuke).MapUid;

            if (nukeMapUid != matchMap)
                continue;

            numNukes++;
        }


        // todo: yeah this logic is all wrong for actually choosing a winner!

        // nukies lost!
        if (numNukes == 0)
            return true;

        var aliveCrew = 0;
        foreach (var player in _match.GetPlayers(match))
        {
            if (player.AttachedEntity == null)
                continue;

            if (HasComp<GhostComponent>(player.AttachedEntity))
                continue;

            if (_mobState.IsIncapacitated(player.AttachedEntity.Value))
                continue;

            // sus hard code
            if ("CrewTeam" != _team.GetTeamOfPlayer(match, player))
                continue;

            aliveCrew++;
        }

        if (aliveCrew == 0)
            return true;

        var aliveNukies = 0;
        foreach (var player in _match.GetPlayers(match))
        {
            if (player.AttachedEntity == null)
                continue;

            if (HasComp<GhostComponent>(player.AttachedEntity))
                continue;

            if (_mobState.IsIncapacitated(player.AttachedEntity.Value))
                continue;

            // sus hard code
            if ("NukieTeam" != _team.GetTeamOfPlayer(match, player))
                continue;

            aliveNukies++;
        }

        if (aliveNukies == 0)
            return true;

        return false;
     }

     public override void Update(float frameTime)
     {
         var query = EntityQueryEnumerator<RetakesRoundComponent, MatchComponent>();
         while (query.MoveNext(out var uid, out var roundComp, out var matchComp))
         {
             Entity<RetakesRoundComponent, MatchComponent> match = (uid, roundComp, matchComp);

             if (!CheckHasWinner(match))
                 continue;

             if (roundComp.HasFinished)
                 continue;

             // round is over!
             _match.StartPhase(uid, MatchPhaseSection.End);

             roundComp.HasFinished = true;
         }
     }
}
