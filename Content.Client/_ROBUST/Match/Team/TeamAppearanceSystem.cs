using System.Linq;
using Content.Shared._ROBUST.Match;
using Content.Shared.Body;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Zombies;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client._ROBUST.Match.Team;

public sealed partial class TeamAppearanceSystem : EntitySystem
{
    [Dependency] private SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeamComponent, GetStatusIconsEvent>(GetIcon);
    }

    private void GetIcon(Entity<TeamComponent> ent, ref GetStatusIconsEvent args)
    {
        if (ent.Comp.CurrentTeam == null)
            return;

        var teamProto = ProtoMan.Index(ent.Comp.CurrentTeam.Value);
        var iconProto = ProtoMan.Index(teamProto.TeamIcon);

        args.StatusIcons.Add(iconProto);
    }

    // public override void Update(float frameTime)
    // {
    //     base.Update(frameTime);
    //
    //     var query = EntityQueryEnumerator<TeamComponent>();
    //
    //     while (query.MoveNext(out var uid, out var teamComp))
    //     {
    //         // if (HasComp<VisualBodyComponent>(uid))
    //         //     return;
    //
    //         if (!TryComp<SpriteComponent>(uid, out var sprite))
    //             continue;
    //
    //         var preferredTeam = teamComp.PreferredTeam;
    //         var currentTeam = teamComp.CurrentTeam;
    //
    //         var team = currentTeam ?? preferredTeam;
    //
    //         if (!ProtoMan.TryIndex(team, out var teamPrototype))
    //             continue;
    //
    //         for (var i = 0; i < sprite.AllLayers.Count(); i++)
    //         {
    //             _sprite.LayerSetColor((uid, sprite), i, teamPrototype.TeamColor);
    //         }
    //     }
    // }
}
