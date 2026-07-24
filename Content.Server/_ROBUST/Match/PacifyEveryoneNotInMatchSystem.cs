using Content.Server.Destructible;
using Content.Server.Popups;
using Content.Shared._ROBUST.Match;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.SSDIndicator;
using Content.Shared.Station;
using Content.Shared.Trigger.Components.Effects;
using Content.Shared.Trigger.Components.Triggers;
using Content.Shared.Trigger.Systems;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._ROBUST.Match;

public sealed partial class PacifyEveryoneNotInMatchSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MatchManagerSystem _match = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private SharedStationSystem _station = default!;
    [Dependency] private MetaDataSystem _meta = default!;
    [Dependency] private PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActivateInWorldEvent>(OnActivatedInWorld, before: [ typeof(TriggerSystem) ]);

        SubscribeLocalEvent<InteractionAttemptEvent>(OnUseOnHand, before: [ typeof(TriggerSystem) ]);

        SubscribeLocalEvent<DestructibleComponent, UseInHandEvent>(OnUseOnHand2, before: [ typeof(TriggerSystem) ]);

        // SubscribeLocalEvent<MatchMapComponent, MatchResetEvent>(OnMatchReset);
        // SubscribeLocalEvent<MatchMapComponent, MatchEndedEvent>(OnMatchEnd);
    }

    // todo: refactor this to be not crazy


    private void OnUseOnHand2(Entity<DestructibleComponent> ent, ref UseInHandEvent args)
    {
        var actor = Comp<ActorComponent>(args.User);

        if (!HasComp<TriggerOnActivateComponent>(ent))
            return;

        if (HasComp<PacifiedComponent>(args.User) && _match.GetAllPlayersInMatches().Contains(actor.PlayerSession))
        {
            args.Handled = true;

            _popup.PopupEntity("So your a dirty cheater huh?", args.User);
        }
    }



    private void OnUseOnHand(ref InteractionAttemptEvent ev)
    {
        if (!HasComp<TriggerOnActivateComponent>(ev.Target))
            return;

        var actor = Comp<ActorComponent>(ev.Uid);

        if (HasComp<PacifiedComponent>(ev.Uid) && _match.GetAllPlayersInMatches().Contains(actor.PlayerSession))
        {
            ev.Cancelled = true;

            _popup.PopupEntity("So your a dirty cheater huh?", ev.Uid);
        }
    }

    private void OnActivatedInWorld(ActivateInWorldEvent ev)
    {
        if (!HasComp<TriggerOnActivateComponent>(ev.Target))
            return;

        var actor = Comp<ActorComponent>(ev.User);

        if (HasComp<PacifiedComponent>(ev.User) && _match.GetAllPlayersInMatches().Contains(actor.PlayerSession))
        {
            ev.Handled = true;

            _popup.PopupEntity("So your a dirty cheater huh?", ev.User);
        }
    }

    public override void Update(float frameTime)
    {
        // var matchPlayers = _match.GetPlayers();

        List<ICommonSession> matchPlayers = [];

        var matches = AllEntityQuery<MatchComponent>();

        while (matches.MoveNext(out var uid, out _))
        {
            var players = _match.GetPlayers(uid);
            matchPlayers.AddRange(players);
        }

        foreach (var session in _player.Sessions)
        {
            if (session.AttachedEntity == null)
                continue;

            // Dont touch people in a match
            if (matchPlayers.Contains(session))
                continue;

            var pacifiedComp = EnsureComp<PacifiedComponent>(session.AttachedEntity.Value);

            pacifiedComp.DisallowAllCombat = true;
            pacifiedComp.DisallowDisarm = true;

            // TODO: Move this to its own system but probably make this system generic that adds / removes any comp you want.
            RemComp<DamageableComponent>(session.AttachedEntity.Value);

            EnsureComp<TeamComponent>(session.AttachedEntity.Value);
        }

        var query = EntityQueryEnumerator<SSDIndicatorComponent>();
        while (query.MoveNext(out var uid, out var ssd))
        {
            if (ssd.IsSSD && _station.IsOnStation(uid))
                QueueDel(uid);
        }

        var queryactor = EntityQueryEnumerator<ActorComponent>();
        while (queryactor.MoveNext(out var uid, out var actor))
        {
            if (_station.IsOnStation(uid))
                _meta.SetEntityName(uid, actor.PlayerSession.Name);
        }
    }
}

