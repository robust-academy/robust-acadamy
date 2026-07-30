using Content.Server._ROBUST.Match;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Robust.Shared.Configuration;
namespace Content.Server._ROBUST.MultiServer;

/// <summary>
/// link between the manager and game!
/// </summary>
public sealed partial class MultiServerSystem : EntitySystem
{
    [Dependency] private MultiServerManager _multiServerManager = default!;
    [Dependency] private MatchControllerSystem _match = default!;
    [Dependency] private MatchManagerSystem _matchManager = default!;
    [Dependency] private ILogManager _log = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IChatManager _chat = default!;

    public ISawmill Sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        Sawmill = _log.GetSawmill("multiserver.test");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Handling only one per tick should be fine!
        if (!_multiServerManager.MessageQueue.TryDequeue(out var messageInfo))
            return;

        _chat.ChatMessageToAll(ChatChannel.OOC, messageInfo.Message, $"({messageInfo.ServerName}) {messageInfo.PlayerName}: {messageInfo.Message}", EntityUid.Invalid, true, false);
    }

}
