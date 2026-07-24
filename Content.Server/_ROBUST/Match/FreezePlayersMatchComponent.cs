
namespace Content.Server._ROBUST.Match;

[RegisterComponent]
public sealed partial class FreezePlayersMatchComponent : Component
{
    [DataField]
    public TimeSpan? UnfreezeTime;
}
