using Content.Server.Chat.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server._ROBUST.Match;

public sealed partial class Announce : IPhase
{
    [DataField(required: true)]
    public string Announcement = "";

    [DataField]
    public Color Color = Color.Yellow;

    // tood: unsure if this works
    [DataField]
    public SoundSpecifier? Sound;

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();
        var chatSystem = entMan.System<ChatSystem>();

        // you have to do this manually because there is no station
        // warn a crew

        var filter = Filter.Empty();
        filter.AddPlayers(matchSystem.GetPlayers(match));
        chatSystem.DispatchFilteredAnnouncement(filter, Announcement, null, null, false, Sound, Color);

        EndPhase(match, entMan);
    }
}
