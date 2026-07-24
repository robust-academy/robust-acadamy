using Robust.Shared.Player;

namespace Content.Server._ROBUST.Match;

[RegisterComponent]
public sealed partial class MatchComponent : Component
{
    [DataField]
    public List<ICommonSession> Players = new();

    [DataField]
    public Enum? CurrentPhaseType;

    [DataField]
    public int CurrentPhaseNumber;

    [DataField]
    public Dictionary<Enum, List<IPhase>> MatchPhases = new();
}


