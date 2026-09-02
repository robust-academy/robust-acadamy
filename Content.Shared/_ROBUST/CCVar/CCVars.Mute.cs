using Robust.Shared.Configuration;

namespace Content.Shared._ROBUST.CCVar;

public partial class RobustCCVars
{
    // format: WORD|MUTE_TIME(in minutes),WORD|MUTE_TIME(in minutes),...
    public static readonly CVarDef<string> BannedWords =
        CVarDef.Create("word_bans.banned_words", "", CVar.REPLICATED);
}
