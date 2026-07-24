namespace Content.Server._ROBUST.Match;

// can only handle one wait at a time.
[RegisterComponent]
public sealed partial class MatchWaitComponent : Component
{
    [DataField]
    public TimeSpan? StopWaitingTime;
}
