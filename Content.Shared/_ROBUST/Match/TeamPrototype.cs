using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Shared._ROBUST.Match;

[Prototype]
public sealed partial class TeamPrototype : IPrototype
{
    /// <inheritdoc/>
    [ViewVariables, IdDataField]
    public string ID { get; private set; } = string.Empty;

    // todo turn into loc id
    [DataField]
    public string TeamName = string.Empty;

    [DataField]
    public Color TeamColor = Color.White;

    [DataField]
    public ProtoId<FactionIconPrototype> TeamIcon;
}
