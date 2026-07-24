using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._ROBUST.Match;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TeamComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<TeamPrototype>? PreferredTeam;

    [DataField, AutoNetworkedField]
    public ProtoId<TeamPrototype>? CurrentTeam;
}
