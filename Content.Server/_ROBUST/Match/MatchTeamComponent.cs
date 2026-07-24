using Robust.Shared.Player;

namespace Content.Server._ROBUST.Match;

[RegisterComponent]
public sealed partial class MatchTeamComponent : Component
{
    [DataField]
    public Dictionary<string, List<ICommonSession>> Teams = new();
}
