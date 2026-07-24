using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._ROBUST.Match;

[Prototype]
public sealed partial class MatchMapPrototype : IPrototype
{
    /// <inheritdoc/>
    [ViewVariables, IdDataField]
    public string ID { get; private set; } = string.Empty;

    // todo turn into loc id
    [DataField]
    public string MapName = string.Empty;

    [DataField]
    public string MapPath = string.Empty;
}
