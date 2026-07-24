using Content.Client.VoiceMask;
using Content.Shared._ROBUST.Match;
using Content.Shared.VoiceMask;
using Robust.Client.UserInterface;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Client._ROBUST.Match.MatchSelectionBUI;

public sealed partial class MatchSelectionBoundUserInterface : BoundUserInterface
{

    [ViewVariables]
    private MatchSelectionWindow? _window;

    public MatchSelectionBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        // this is an insane fix for the window reopening...
        // todo: fix this
        var uiControllerEntMapId = EntMan.System<SharedTransformSystem>().GetMapId(Owner);
        var playerMapId = EntMan.System<SharedTransformSystem>().GetMapId(PlayerManager.LocalEntity!.Value);

        if (!uiControllerEntMapId.Equals(playerMapId))
            return;

        _window = this.CreateWindow<MatchSelectionWindow>();
        _window.AddMatches();

        _window.OnMatchStarted += OnMatchStarted;

        _window.OnClose += Close;
    }

    private void OnMatchStarted((ProtoId<MatchPrototype> MatchProtoId,  MatchSettings settings) args)
    {
        SendPredictedMessage(new MatchAttemptStart(args.MatchProtoId, args.settings));

        Close();

        _window = null;

        // _window?.Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
