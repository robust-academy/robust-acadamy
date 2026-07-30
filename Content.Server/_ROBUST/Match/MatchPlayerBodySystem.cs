using System.Linq;
using System.Numerics;
using Content.Server._ROBUST.Match.Spawners;
using Content.Server.GameTicking;
using Content.Shared.Administration.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._ROBUST.Match;

public sealed class MatchPlayerBodySystem : EntitySystem
{

    public void DeleteAllBodies(EntityUid match)
    {
        if (!TryComp<MatchPlayerBodyComponent>(match, out var playerBody))
            return;

        foreach (var body in playerBody.OldBodies.Values)
        {
            QueueDel(body);
        }
    }

}

public sealed partial class BringPlayerBodiesToNullSpace : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();
        var transformSys = entMan.System<SharedTransformSystem>();

        var matchBodyComp = entMan.EnsureComponent<MatchPlayerBodyComponent>(match);
        var players = matchSystem.GetPlayers(match);

        foreach (var player in players)
        {
            var oldBody = player.AttachedEntity;
            matchSystem._player.SetAttachedEntity(player, null, true);

            if (oldBody == null)
                continue;

            // error check
            if (matchBodyComp.OldBodies.ContainsKey(player))
                throw new Exception($"Player {player} already has a body stored!");

            matchBodyComp.OldBodies.Add(player, oldBody.Value);

            transformSys.DetachEntity(oldBody.Value);
        }

        EndPhase(match, entMan);
    }
}

public sealed partial class DeletePlayerBodies : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();
        var players = matchSystem.GetPlayers(match);

        foreach (var player in players)
        {
            var oldBody = player.AttachedEntity;

            if (oldBody == null)
                throw new Exception("Player body is null not good!!!!");

            matchSystem._player.SetAttachedEntity(player, null, true);

            if (oldBody != null)
                entMan.DeleteEntity(oldBody.Value);
        }

        EndPhase(match, entMan);
    }
}

public sealed partial class BringPlayersToLobby : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();
        var transform = entMan.System<SharedTransformSystem>();
        var revive = entMan.System<RejuvenateSystem>();

        var matchBodyComp = entMan.EnsureComponent<MatchPlayerBodyComponent>(match);
        var players = matchSystem.GetPlayers(match);

        // todo fix this
        var spawnEnt = entMan.AllComponents<RespawnLocationComponent>()[0].Uid;
        var spawnCords = entMan.GetComponent<TransformComponent>(spawnEnt).Coordinates;

        foreach (var player in players)
        {
            var oldBody = matchBodyComp.OldBodies[player];

            // todo fix this
            revive.PerformRejuvenate(oldBody);

            matchSystem._player.SetAttachedEntity(player, oldBody, true);

            matchBodyComp.OldBodies.Remove(player);

            transform.SetCoordinates(oldBody, spawnCords);
        }

        EndPhase(match, entMan);
    }
}

public sealed partial class RespawnPlayersToLobby : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();
        var gameTicker = entMan.System<GameTicker>();

        var players = matchSystem.GetPlayers(match);

        foreach (var player in players)
        {
            gameTicker.Respawn(player);
        }

        EndPhase(match, entMan);
    }
}

// MobObserver

public sealed partial class TransferMindsToGhosts : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();
        var transform = entMan.System<SharedTransformSystem>();

        var players = matchSystem.GetPlayers(match);
        var map = matchSystem.GetMap(match);

        foreach (var player in players)
        {
            // todo: do a check to ensure they are not attached, if they are throw error.

            var mapId = entMan.GetComponent<MapComponent>(map).MapId;

            var spawnEnt = entMan.AllComponents<ObserverLocationComponent>().Where(ent => entMan.GetComponent<TransformComponent>(ent.Uid).MapID.Equals(mapId)).Shuffle().First();
            var spawnCords = entMan.GetComponent<TransformComponent>(spawnEnt.Uid).Coordinates;

            var mapLocation = new MapCoordinates(new Vector2(), mapId);

            var ghostBody = entMan.Spawn("MatchObserver", mapLocation);

            transform.SetCoordinates(ghostBody, spawnCords);

            matchSystem._player.SetAttachedEntity(player, ghostBody, true);
        }

        EndPhase(match, entMan);
    }
}

