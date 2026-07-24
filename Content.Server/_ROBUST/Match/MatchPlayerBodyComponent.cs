using Robust.Shared.Player;

namespace Content.Server._ROBUST.Match;

[RegisterComponent]
public sealed partial class MatchPlayerBodyComponent : Component
{
    // [DataField]
    public Dictionary<ICommonSession, EntityUid> OldBodies = new();
}
