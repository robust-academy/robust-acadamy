using Content.Client._ROBUST.Match.Timer;
using Content.Shared._ROBUST.Match;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Network;

namespace Content.Client._ROBUST.Match.Kits;

// TODO: Use UI controller?
public sealed partial class KitUIController : EntitySystem
{
    [Dependency] private IClientNetManager _net = default!;

    private KitSelectionWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<KitsToChooseFromMessage>(OnKitToChooseFrom);
    }

    private void OnKitToChooseFrom(KitsToChooseFromMessage msg, EntitySessionEventArgs args)
    {
        // If a window is already open, close it
        _window?.Close();

        _window = new KitSelectionWindow(msg.Kits);
        _window.CountdownTimer.TimeFormat = TimeFormat.SecondsMilliseconds;
        _window.CountdownTimer.StartCountdown(msg.MaximumSelectionTime);
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
        _window.OnKitChosen += OnKitSelected;
        _window.CountdownTimer.OnTimerFinished += () => _window?.Visible = false; // todo: change this, close doesn't work because your closing in an update loop
    }

    private void OnKitSelected(int args)
    {
        var kit = new KitChosenMessage();
        kit.Kit = args;
        _net.ClientSendMessage(kit);
        _window?.Close();
    }
}
