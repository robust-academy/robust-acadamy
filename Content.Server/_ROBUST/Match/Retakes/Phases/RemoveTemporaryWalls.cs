namespace Content.Server._ROBUST.Match.Retakes.Phases;


public sealed partial class RemoveTemporaryWalls : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchManager = entMan.System<MatchManagerSystem>();

        var walls = entMan.EntityQueryEnumerator<TemporaryWallComponent>();

        var matchMap = matchManager.GetMap(match);

        foreach (var wall in walls)
        {
            var nukeMapUid = entMan.GetComponent<TransformComponent>(wall).MapUid;

            if (nukeMapUid != matchMap)
                continue;

            entMan.DeleteEntity(wall);
        }

        EndPhase(match, entMan);
    }
}
