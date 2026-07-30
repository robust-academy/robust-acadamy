using Content.Shared.Verbs;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._ROBUST.Match;

public sealed partial class MatchSelectionSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MatchSelectionComponent, GetVerbsEvent<InteractionVerb>>(OnVerb);

    }

    private void OnVerb(Entity<MatchSelectionComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Can't pass args from a ref event inside of lambdas
        var user = args.User;

        args.Verbs.Add(new InteractionVerb()
        {
            Text = Loc.GetString("Open match UI"),
            Act = () => _ui.TryToggleUi(ent.Owner, MatchSelectionUiKey.Key, user),
        });
    }
}

[Serializable, NetSerializable]
public sealed class MatchAttemptStart(ProtoId<MatchPrototype> matchProtoId, MatchSettings settings) : BoundUserInterfaceMessage
{
    public readonly ProtoId<MatchPrototype> MatchProtoId = matchProtoId;

    public readonly MatchSettings MatchSettings = settings;
}

[Serializable, NetSerializable]
public enum MatchSelectionUiKey : byte
{
    Key
}
