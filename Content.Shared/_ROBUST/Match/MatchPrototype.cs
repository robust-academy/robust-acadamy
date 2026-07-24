using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Shared._ROBUST.Match;

[Prototype]
public sealed partial class MatchPrototype : IPrototype
{
    /// <inheritdoc/>
    [ViewVariables, IdDataField]
    public string ID { get; private set; } = string.Empty;

    // todo turn into loc id
    [DataField]
    public string MatchName = string.Empty;

    // todo: rename to match ent protoid
    [DataField]
    public EntProtoId MatchEntProtoId;
}
