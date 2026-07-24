using System.Linq;
using Content.Shared._ROBUST.Match;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._ROBUST.Match;

public sealed partial class MatchTeamsSystem : EntitySystem
{

    public override void Initialize()
    {

    }

    public List<List<ICommonSession>> GetTeamsAndTheirPlayers(EntityUid match)
    {
        var teamComp = Comp<MatchTeamComponent>(match);

        List<List<ICommonSession>> teams = new();

        foreach (var team in teamComp.Teams)
        {
            teams.Add(team.Value);
        }

        return teams;
    }

    public List<string> GetTeams(EntityUid match)
    {
        var teamComp = Comp<MatchTeamComponent>(match);

        return teamComp.Teams.Keys.ToList();
    }

    public string GetTeamOfPlayer(EntityUid match, ICommonSession player)
    {
        var teamComp = Comp<MatchTeamComponent>(match);

        foreach (var team in teamComp.Teams)
        {
            if (team.Value.Contains(player))
                return team.Key;
        }

        throw new Exception("Player was not on team!");
    }
}

public sealed partial class ChooseTeams : IPhase
{
    private MatchManagerSystem _match = default!;

    [DataField(required: true)]
    public Dictionary<string, int> Teams;

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        // IoCManager.InjectDependencies(this);

        _match = entMan.System<MatchManagerSystem>();

        if (Teams.Count == 0)
            throw new Exception("There has to be at least one team");

        var players = _match.GetPlayers(match).Shuffle().ToList();

        var teamComp = entMan.EnsureComponent<MatchTeamComponent>(match);

        // todo: combine this with the other for loop
        foreach (var team in Teams)
        {
            teamComp.Teams.Add(team.Key, new List<ICommonSession>());
        }


        List<ICommonSession> addedPlayers = [];

        // Match players to their teams (if possible).
        foreach (var player in players)
        {
            var body = player.AttachedEntity;

            if (!entMan.TryGetComponent<TeamComponent>(body, out var playerTeamComp))
                continue;

            if (playerTeamComp.PreferredTeam == null)
                continue;

            if (!teamComp.Teams.ContainsKey(playerTeamComp.PreferredTeam))
                continue;

            if (teamComp.Teams[playerTeamComp.PreferredTeam].Count >= Teams[playerTeamComp.PreferredTeam])
                continue;

            teamComp.Teams[playerTeamComp.PreferredTeam].Add(player);
            addedPlayers.Add(player);
        }

        foreach (var player in players)
        {
            if (addedPlayers.Contains(player))
                continue;

            var foundTeam = false;
            foreach (var team in teamComp.Teams)
            {
                if (foundTeam)
                    continue;
                // its full
                if (team.Value.Count >= Teams[team.Key])
                    continue;

                team.Value.Add(player);
                foundTeam = true;
            }
        }



        EndPhase(match, entMan);
    }
}

