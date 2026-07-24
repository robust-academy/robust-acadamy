using Robust.Shared.Prototypes;

namespace Content.Shared._ROBUST.Match;

[Prototype]
public sealed partial class MapPackPrototype : IPrototype
{
    /// <inheritdoc/>
    [ViewVariables, IdDataField]
    public string ID { get; private set; } = string.Empty;

    [DataField]
    public List<ProtoId<MatchMapPrototype>> Maps = new();
}
