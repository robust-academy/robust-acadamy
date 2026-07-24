using Content.Server._ROBUST.DebugSystem;

namespace Content.Server._ROBUST.Match;

// TODO: need some way to see if a phase doesn't end when its supposed to.

[ImplicitDataDefinitionForInheritors]
public abstract partial class IPhase
{
    public virtual void RunPhase(EntityUid match, IEntityManager entMan)
    {

    }

    protected virtual void EndPhase(EntityUid match, IEntityManager entMan)
    {
        entMan.System<DebugMatch>().Serialize(match, NextPhase.ToString());

        entMan.System<MatchManagerSystem>().EndCurrentPhase(match);
    }

    [DataField]
    public Enum NextPhase = MatchPhaseSection.S_ContinueCurrentPhase;
}

[ByRefEvent]
public record struct PhaseEndedEvent;

public enum MatchPhaseSection
{
    /* Special */
    S_ContinueCurrentPhase, // Special type, just continue the current phase your in!
    S_EndCurrentPhase,      // Special type, end the current phase.

    /* Normal sections */
    Start,
    End,

    RoundStart,
    RoundEnd,
}
