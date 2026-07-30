using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Shared._ROBUST.CCVar;
using Content.Shared.Chat;
using Robust.Shared.Configuration;

namespace Content.Server._ROBUST.MultiServer;

public sealed partial class MultiServerManager
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private ILogManager _log = default!;

    public ISawmill Sawmill = default!;

    private readonly CancellationTokenSource _cts = new();

    private readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true
    };

    public ConcurrentQueue<POSTMessageInfo> MessageQueue = new();

    private HttpClient _httpClient = default!;

    public void Initialize()
    {
        Task.Run(HandleIncomingRequests);

        _httpClient = new HttpClient();

        Sawmill = _log.GetSawmill("multiserver.test");
    }

    private async Task HandleIncomingRequests()
    {
        var token = _cts.Token;

        using var listener = new HttpListener();
        listener.Prefixes.Add("http://+:6767/");

        listener.Start();

        while (!token.IsCancellationRequested)
        {
            var ctx = await listener.GetContextAsync();

            Sawmill.Info("Got message!");

            var request = ctx.Request;

            try
            {
                var messageInfo = await JsonSerializer.DeserializeAsync<POSTMessageInfo>(request.InputStream,
                    JsonOptions,
                    cancellationToken: token);

                if (messageInfo is null)
                    continue; // throw error?

                MessageQueue.Enqueue(messageInfo);
            }
            catch
            {
                Sawmill.Error("Could not parse message");
            }
        }
    }

    public void SendMessageOtherServers(string message, string player)
    {
        var myName = _cfg.GetCVar(RobustCCVars.ServerName);

        var servers = _cfg.GetCVar(RobustCCVars.MutliServerOtherServers).Split(",");

        foreach (var server in servers)
        {
            var name = server.Split("|")[0];
            var ip = server.Split("|")[1];
            var port = ushort.Parse(server.Split("|")[2]);

            if (name.Equals(myName))
                continue;

            SendMessage(ip, port, message, player);
        }
    }

    public void SendMessage(string ip, ushort port, string message, string player)
    {
        var postRequest = new POSTMessageInfo
        {
            ServerName = _cfg.GetCVar(RobustCCVars.ServerName),
            PlayerName = player,
            Message = message,
        };

        Task.Run(() => _httpClient.PostAsJsonAsync($"http://{ip}:{6767}", postRequest, JsonOptions));
    }

    // todo: make it so this will be called during BaseServer.Cleanup!
    public void Shutdown()
    {
        _cts.Cancel();
    }
}
