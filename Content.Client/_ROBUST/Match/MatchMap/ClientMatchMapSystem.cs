using Content.Client._ROBUST.Match.MatchSelectionBUI;
using Content.Client._ROBUST.Match.MatchSelectionBUI.SettingWidgets;
using Content.Shared._ROBUST.Match;
using Robust.Shared.Prototypes;

namespace Content.Client._ROBUST.Match.MatchMap;

public sealed class ClientMatchMapSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MatchMapComponent, GetSettingsFromMatchEvent>(OnGetSettingsFromMatchEvent);
    }

    private void OnGetSettingsFromMatchEvent(Entity<MatchMapComponent> ent, ref GetSettingsFromMatchEvent args)
    {
        List<ProtoId<MatchMapPrototype>> mapsProtoId = new();

        foreach (var pack in ent.Comp.AvailableMaps)
        {
            mapsProtoId.AddRange(ProtoMan.Index(pack).Maps);
        }

        Dictionary<string, string> settingsDictionary = new();
        foreach (var map in mapsProtoId)
        {
            var prototype = ProtoMan.Index(map);
            settingsDictionary.Add(prototype.MapName, map);
        }

        var dropdown = new DropdownSelectorSetting(ent.Comp.SettingName, settingsDictionary);

        args.Settings.Add(dropdown);
    }
}
