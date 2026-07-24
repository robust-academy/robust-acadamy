using System.Linq;
using Content.Shared.Implants;
using Content.Shared.Radio.Components;
using Content.Shared.Verbs;
using Robust.Shared.Prototypes;

namespace Content.Shared._ROBUST.Match;

public sealed class TeamSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TeamComponent, GetVerbsEvent<Verb>>(OnGetVerb);
    }

    private void OnGetVerb(Entity<TeamComponent> entity, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (args.User != args.Target)
            return;

        var allTeams = ProtoMan.EnumeratePrototypes<TeamPrototype>();

        byte index = 0;
        foreach (var team in allTeams)
        {
            // This is because Act wont work with index.
            // Needs it to be saved in the loop.
            var currIndex = index;
            var verb = new Verb
            {
                Priority = currIndex,
                Category = VerbCategory.TeamCategory,
                Disabled = team.ID == entity.Comp.PreferredTeam,
                Act = () =>
                {
                    entity.Comp.PreferredTeam = team.ID;
                    Dirty(entity);
                },
                Text = team.TeamName,
            };
            args.Verbs.Add(verb);
            index++;
        }
    }
}
