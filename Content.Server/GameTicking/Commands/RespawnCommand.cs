using System.Linq;
using Content.Server._ROBUST.Match;
using Content.Server.Administration;
using Content.Server.Mind;
using Content.Shared.Administration;
using Content.Shared.Players;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Network;

namespace Content.Server.GameTicking.Commands
{
    [AnyCommand] // ROBUST
    sealed partial class RespawnCommand : LocalizedEntityCommands
    {
        [Dependency] private IPlayerManager _player = default!;
        [Dependency] private IPlayerLocator _locator = default!;
        [Dependency] private GameTicker _gameTicker = default!;
        [Dependency] private MindSystem _mind = default!;
        [Dependency] private MatchManagerSystem _match = default!; // ROBUST

        public override string Command => "respawn";

        public override async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            // ROBUST START
            if (args.Length != 0) // only can respawn yourself
                return;

            if (shell.Player == null)
                return;

            var match = _match.GetMatchFromPlayer(shell.Player);

            if (match != null)
            {
                _match.DeleteMatchAndRespawnPlayers(match.Value);
            }
            // ROBUST END

            var player = shell.Player;
            if (args.Length > 1)
            {
                shell.WriteError(Loc.GetString("cmd-respawn-invalid-args"));
                return;
            }

            NetUserId userId;
            if (args.Length == 0)
            {
                if (player == null)
                {
                    shell.WriteError(Loc.GetString("cmd-respawn-no-player"));
                    return;
                }

                userId = player.UserId;
            }
            else
            {
                var located = await _locator.LookupIdByNameOrIdAsync(args[0]);

                if (located == null)
                {
                    shell.WriteError(Loc.GetString("cmd-respawn-unknown-player"));
                    return;
                }

                userId = located.UserId;
            }

            if (!_player.TryGetSessionById(userId, out var targetPlayer))
            {
                if (!_player.TryGetPlayerData(userId, out var data))
                {
                    shell.WriteError(Loc.GetString("cmd-respawn-unknown-player"));
                    return;
                }

                _mind.WipeMind(data.ContentData()?.Mind);
                shell.WriteError(Loc.GetString("cmd-respawn-player-not-online"));
                return;
            }

            _gameTicker.Respawn(targetPlayer);
        }

      public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length != 1)
                return CompletionResult.Empty;

            var options = _player.Sessions.OrderBy(c => c.Name).Select(c => c.Name).ToArray();

            return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-respawn-player-completion"));
        }
    }
}
