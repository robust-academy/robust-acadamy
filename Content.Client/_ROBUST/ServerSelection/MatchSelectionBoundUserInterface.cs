using Content.Client._ROBUST.Match.MatchSelectionBUI;
using Content.Shared._ROBUST.CCVar;
using Content.Shared._ROBUST.Match;
using Robust.Client;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client._ROBUST.ServerSelection;

public sealed partial class ServerSelectionBoundUserInterface : BoundUserInterface
{
    [Dependency] private IBaseClient _client = default!;
    [Dependency] private IConfigurationManager _cfg = default!;

    [ViewVariables]
    private ServerSelectionWindow? _window;

    public ServerSelectionBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ServerSelectionWindow>();

        // _window.OnMatchStarted += OnMatchStarted;

        _window.OnClose += Close;
    }

    private void OnMatchStarted((ProtoId<MatchPrototype> MatchProtoId,  MatchSettings settings) args)
    {
        // _client.DisconnectFromServer("Force Quit");
        // _client.ConnectToServer("45.151.153.61\t", 1212);

        SendPredictedMessage(new MatchAttemptStart(args.MatchProtoId, args.settings));

        Close();

        _window = null;
        // _gameController.Redial($"ss14://{ev.IP}:{ev.Port}", "Joining new server");
        // _window?.Close();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
