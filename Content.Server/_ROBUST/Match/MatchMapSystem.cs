using System.Runtime.InteropServices;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._ROBUST.Match;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._ROBUST.Match;
public sealed partial class MatchMapSystem : EntitySystem
{
    [Dependency] private MapLoaderSystem _loader = default!;
    [Dependency] private MetaDataSystem _metaDataSystem = default!;
    [Dependency] private SharedMapSystem _maps = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private AtmosphereSystem _atmosphere = default!;
    [Dependency] private ISharedPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MatchMapComponent, MatchStartedEvent>(OnMatchStart);
        // SubscribeLocalEvent<MatchMapComponent, MatchResetEvent>(OnMatchReset);
        // SubscribeLocalEvent<MatchMapComponent, MatchEndedEvent>(OnMatchEnd);
    }

    private void OnMatchStart(Entity<MatchMapComponent> ent, ref MatchStartedEvent args)
    {
        // todo: SANITIZE THIS!!! clients can send whatever the flip they want

        if (args.MatchSettings == null || !args.MatchSettings.Settings.TryGetValue(ent.Comp.SettingName, out var setting))
            return;

        var selectedMap = ProtoMan.Index<MatchMapPrototype>(setting);

        ent.Comp.ChosenMap = selectedMap.MapPath;
    }

    // private void OnMatchReset(Entity<MatchMapComponent> ent, ref MatchResetEvent args)
    // {
    //     // QueueDel(ent.Comp.GridUid);
        // QueueDel(ent.Comp.MapUid);

        // var path = new ResPath(ent.Comp.MapPath);
        // var mapUid = _maps.CreateMap(out var mapId);
        //
        // if (!_loader.TryLoadGrid(mapId, path, out var grid))
        // {
        //     QueueDel(mapUid);
        //     throw new Exception($"Failed to load admin arena");
        // }
        //
        // ent.Comp.GridUid = grid.Value.Owner;
        // ent.Comp.MapUid = mapUid;
    // }

    // private void OnMatchEnd(Entity<MatchMapComponent> ent, ref MatchEndedEvent args)
    // {
        // QueueDel(ent.Comp.GridUid);
        // QueueDel(ent.Comp.MapUid);
    // }
}

public sealed partial class CreateMap : IPhase
{
    [DataField]
    public string MapName = "defaultMap";

    [DataField]
    public string? MapPath;

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var mapSystem = entMan.System<SharedMapSystem>();
        var mapLoader = entMan.System<MapLoaderSystem>();

        var comp = entMan.EnsureComponent<MatchMapComponent>(match);

        if (comp.Maps.ContainsKey(MapName) || comp.Grids.ContainsKey(MapName))
            throw new Exception($"Map {MapName} already exists.");

        var map = mapSystem.CreateMap(out var mapId);

        var mapPath = MapPath;
        if (mapPath == null)
            mapPath = comp.ChosenMap;

        if (mapPath == null)
            throw new Exception("Map is null! Should not be possible you made a mistake.");

        if (!mapLoader.TryLoadGrid(mapId, new ResPath(mapPath), out var grid))
            throw new Exception($"Failed to load map {mapPath}");

        comp.Maps.Add(MapName, map);
        comp.Grids.Add(MapName, grid.Value);

        EndPhase(match, entMan);
    }
}

// todo: make sure it deletes all the entities on the map as well
public sealed partial class DeleteMap : IPhase
{
    [DataField]
    public string MapName = "defaultMap";

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var mapSystem = entMan.System<SharedMapSystem>();

        var comp = entMan.GetComponent<MatchMapComponent>(match);

        var oldMapId = entMan.GetComponent<MapComponent>(comp.Maps[MapName]).MapId;

        mapSystem.DeleteMap(oldMapId);

        comp.Maps.Remove(MapName);
        comp.Grids.Remove(MapName);

        EndPhase(match, entMan);
    }
}
