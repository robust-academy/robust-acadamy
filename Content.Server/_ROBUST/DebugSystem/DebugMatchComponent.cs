namespace Content.Server._ROBUST.DebugSystem;

[RegisterComponent]
public sealed partial class DebugMatchComponent : Component
{
    [DataField]
    public int DebugNumber = -99;

    /// <summary>
    /// +1 every time the match is serialized for debugging
    /// </summary>
    [DataField]
    public int SerializationNumber = -1;
}
