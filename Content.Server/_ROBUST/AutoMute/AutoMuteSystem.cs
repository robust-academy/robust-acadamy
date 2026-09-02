using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Content.Server.Administration.Notes;
using Content.Server.Chat.Managers;
using Content.Shared._ROBUST.CCVar;
using Content.Shared.Chat;
using Content.Shared.Database;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Server._ROBUST.AutoMute;

public sealed partial class AutoMuteSystem : EntitySystem
{
    [Dependency] private IAdminNotesManager _notes = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    private const string NoteMutedTag = "-- MUTED --";
    private readonly TimeSpan HighSeverityNoteTime = TimeSpan.FromMinutes(1);
    private readonly SoundPathSpecifier MutedSound = new("/Audio/Effects/adminhelp.ogg");

    private List<(string Word, TimeSpan MuteTime)> BannedWords = new();

    public override void Initialize()
    {
        base.Initialize();

        // todo, have this update whenever the ccvar is changed (it should never be changed mid round though...)
        var allWords = _cfg.GetCVar(RobustCCVars.BannedWords).Split(",");

        foreach (var word in allWords)
        {
            var split = (word.Split("|")[0].ToLower(), TimeSpan.FromMinutes(int.Parse(word.Split("|")[1])));
            BannedWords.Add(split);
            BannedWords.Add((split.Item1 + "s", split.Item2)); // sussy hardcode!
        }
    }

    [SubscribeLocalEvent]
    private void OnSendMessageAttempt(ref ROBUSTBeforeMessageEvent args)
    {
        if (args.Session == null)
            return;

        var session = args.Session;

        if (IsMuted(session))
        {
            args.Cancelled = true;
            return;
        }

        if (SaidBannedWord(args.Message, out var muteTime))
        {
            args.Cancelled = true;
            var message = args.Message;
            ApplyMute(session, message, muteTime.Value);
            // todo: make this more expansive, e.g if you've done it enough you get kicked.

            _chat.DeleteMessagesBy(session.UserId);

            var muteMessage = $"You have been muted for {muteTime.Value.TotalMinutes} minutes. See admin remarks.";
            _chat.ChatMessageToOne(ChatChannel.Server, muteMessage, muteMessage, EntityUid.Invalid, false, session.Channel);

            if (session.AttachedEntity != null)
                _audio.PlayLocal(MutedSound, session.AttachedEntity.Value, session.AttachedEntity.Value);

            // todo: make it so you say "Good game!" if you get muted

            return;
        }
    }

    // todo: right now this will only find the first instance of a banned word, if you say a slur and something
    //  less bad, it might find the less bad one first
    private bool IsMuted(ICommonSession session)
    {
        // https://stackoverflow.com/questions/22628087/calling-async-method-synchronously
        var notes = Task.Run(() => _notes.GetAllAdminRemarks(session.UserId)).GetAwaiter().GetResult();

        foreach (var note in notes)
        {
            if (note.Deleted || note.ExpirationTime < DateTime.UtcNow)
                continue;

            if (note.Message.Contains(NoteMutedTag))
                return true;
        }

        return false;
    }

    // This might act strangely if there are multiple words that are nested (E.g "test" and "tests"), "test" might
    // get caught before "tests"
    private bool SaidBannedWord(string argsMessage, [NotNullWhen(true)] out TimeSpan? muteTime)
    {
        muteTime = null;
        var messageLower = argsMessage.ToLower();
        var wordsInMessage = messageLower.Split(" ");

        foreach (var bannedWord in BannedWords)
        {
            foreach (var word in wordsInMessage)
            {
                if (word != bannedWord.Word)
                    continue;

                muteTime = bannedWord.MuteTime;
                return true;
            }
        }

        return false;
    }

    private async void ApplyMute(ICommonSession session, string message, TimeSpan muteTime)
    {
        var muteMessage = $"{NoteMutedTag}\nMuted for message:\n\"{message}\"\nAppeal on discord.";

        var severity = NoteSeverity.Medium;

        if (muteTime > HighSeverityNoteTime)
            severity = NoteSeverity.High;

        await _notes.AddAdminRemark(session, session.UserId, NoteType.Note, muteMessage, severity, false, DateTime.UtcNow + muteTime);
    }
}

[ByRefEvent]
public record struct ROBUSTBeforeMessageEvent(ICommonSession? Session, string Message, bool Cancelled = false);
