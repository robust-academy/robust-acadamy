using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._ROBUST.Match;

[RegisterComponent]
public sealed partial class MatchKitComponent : Component
{
     /// <summary>
     ///     Player session -> List of 3 kits, each of which has a dictionary of name -> list of entities in that section.
     /// </summary>
     [DataField]
     public Dictionary<ICommonSession, List<Dictionary<string, List<EntProtoId>>>> AvailableKits = new();

     [DataField]
     public Dictionary<ICommonSession, int> ChosenKits = new();

     [DataField]
     public TimeSpan MaximumSelectionTime;

     [DataField]
     public TimeSpan? StopWaitingKitTime;
}
