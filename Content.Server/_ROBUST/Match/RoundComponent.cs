using Robust.Shared.Player;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._ROBUST.Match;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class RoundComponent : Component
{
     [DataField]
     public int RoundNumber = 0;

     [DataField]
     public int FirstToPoints = 2;

     [DataField]
     public Dictionary<string, int> TeamToRoundsWon = new();

     [DataField]
     public TimeSpan MaxRoundLength = TimeSpan.FromMinutes(5);

     [DataField(customTypeSerializer:typeof(TimeOffsetSerializer)), AutoPausedField]
     public TimeSpan? CurrentRoundStartTime;
}
