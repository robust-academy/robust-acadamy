using Content.Shared._ROBUST.Match;
using Robust.Client.UserInterface;

namespace Content.Client._ROBUST.Match.MatchSelectionBUI.SettingWidgets;

public interface ISettingsWidget
{
    void GetSetting(ref MatchSettings matchSettings);
}

