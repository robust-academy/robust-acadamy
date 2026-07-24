using Content.Shared._ROBUST.Match;
using Robust.Shared.Network;

namespace Content.Client._ROBUST.Match.MatchConfirm;

// TODO: Use UI controller?
public sealed partial class MatchConfirmUIController : EntitySystem
{
    [Dependency] private IClientNetManager _net = default!;

    private MatchConfirmWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RequestMatchStart>(OnRequestMatchStart);
    }

    private void OnRequestMatchStart(RequestMatchStart msg, EntitySessionEventArgs args)
    {
        // If a window is already open, close it
        _window?.Close();

        _window = new MatchConfirmWindow(msg.Username);
        _window.OpenCentered();

        _window.OnConfirm += OnConfirm;
    }

    private void OnConfirm()
    {
        var msg = new ConfirmMatchStart();
        _net.ClientSendMessage(msg);
        _window?.Close();
    }
}
