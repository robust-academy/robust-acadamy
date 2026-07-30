using System.Net.Http;
using System.Text.Json.Serialization;
using Content.Shared._ROBUST.Match;

namespace Content.Server._ROBUST.MultiServer;

public sealed class POSTMessageInfo
{
    [JsonIgnore]
    public HttpMethod RequestMethod => HttpMethod.Post;

    [JsonInclude]
    public required string ServerName;

    [JsonInclude]
    public required string PlayerName;

    [JsonInclude]
    public required string Message;
}
