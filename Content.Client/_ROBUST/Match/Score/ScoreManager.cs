using System.Linq;
using Content.Client._ROBUST.Match.Timer;
using Content.Shared._ROBUST.Match;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Network;

namespace Content.Client._ROBUST.Match.Score;

public sealed partial class ScoreManager
{
    [Dependency] private IClientNetManager _net = default!;
    [Dependency] private IPlayerManager _player = default!;

    private ScoreBarUI? ScoreUIElement;

    public void Initialize()
    {
        _net.RegisterNetMessage<CurrentScoreMessage>(OnCurrentScoreUpdate);
        _net.RegisterNetMessage<HideScoreboard>(OnHideScoreboard);
        _net.RegisterNetMessage<SetTimerScoreMessage>(OnSetTimerScoreMessage);
        _net.RegisterNetMessage<SetDebugInfo>(OnSetDebugInfo);
    }

    private void OnSetTimerScoreMessage(SetTimerScoreMessage message)
    {
        if (ScoreUIElement == null)
            throw new Exception("UI Element is null, should not be possible");

        ScoreUIElement.RoundTime.TimeFormat = TimeFormat.MinutesSecondsMilliseconds;
        ScoreUIElement.RoundTime.StartCountdown(message.TimerLength);
    }

    private void OnCurrentScoreUpdate(CurrentScoreMessage message)
    {
        if (ScoreUIElement == null)
            throw new Exception("UI Element is null, should not be possible");

        ScoreUIElement.Visible = true;

        if (_player.LocalSession == null)
            throw new Exception("Local session is null!");

        if (message.Scores.Count != 0)
        {
            // var localSessionGuid = _player.LocalSession.UserId.UserId;


            ScoreUIElement.YourScore.Text = message.Scores[message.YourTeam].ToString();

            // todo: only works for 2 teams
            var opponentsScore = message.Scores.Single(pair => pair.Key != message.YourTeam).Value;

            ScoreUIElement.OpponentsScore.Text = opponentsScore.ToString();
        }
        else
        {
            ScoreUIElement.YourScore.Text = "-";
            ScoreUIElement.YourScore.Text = "-";
        }

        ScoreUIElement.Footer.Text = $"First to {message.FirstToPoints}!";
    }

    private void OnSetDebugInfo(SetDebugInfo message)
    {
        if (ScoreUIElement == null)
            throw new Exception("UI Element is null, should not be possible");

        ScoreUIElement.DebugInfo.Text = message.DebugInfoNumber.ToString();
    }

    public void SetUIElement(ScoreBarUI scoreUIElement)
    {
        ScoreUIElement?.Orphan();
        ScoreUIElement?.Dispose(); // todo fix thiss

        ScoreUIElement = scoreUIElement;
    }

    private void OnHideScoreboard(HideScoreboard message)
    {
        if (ScoreUIElement == null)
            throw new Exception("UI Element is null, should not be possible");

        // todo: fix this later, for now we need it for debugging
        // ScoreUIElement.Visible = false;
    }
}

