using Robust.Shared.Configuration;

namespace Content.Shared._ROBUST.CCVar;

[CVarDefs]
public sealed partial class RobustCCVars
{
    // format:
    // SERVERNAME|IPADDRESS,SERVERNAME|IPADDRESS|PORT
    public static readonly CVarDef<string> MutliServerOtherServers =
        CVarDef.Create("multi_server.other_servers", "", CVar.REPLICATED);

    public static readonly CVarDef<string> ServerName =
        CVarDef.Create("multi_server.server_name", "", CVar.REPLICATED);

    // format: WORD|MUTE_TIME(in minutes),WORD|MUTE_TIME(in minutes),...
    public static readonly CVarDef<string> BannedWords =
        CVarDef.Create("word_bans.banned_words", "", CVar.REPLICATED);
}
