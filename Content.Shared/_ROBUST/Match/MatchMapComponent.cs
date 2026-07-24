using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ROBUST.Match;

/// <summary>
/// For now, maps and grids will be the same. Should be split up in the future!
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MatchMapComponent : Component
{
    // todo: maybe this should be in a shared system?
    [DataField]
    public string SettingName = "MatchMapSetting";

    [DataField]
    public List<ProtoId<MapPackPrototype>> AvailableMaps = new();

    // is a res path.
    // todo: Should update this to be a dictionary if we want multiple maps at some point!
    [DataField]
    public string? ChosenMap;

    [DataField]
    public Dictionary<string, EntityUid> Maps = new();

    [DataField]
    public Dictionary<string, EntityUid> Grids = new();
}
