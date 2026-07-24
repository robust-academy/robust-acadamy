using System.IO;
using Content.Server._ROBUST.Match;
using Content.Shared._ROBUST.Match;
using Robust.Shared.ContentPack;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Server._ROBUST.DebugSystem;

public sealed partial class DebugMatch : EntitySystem
{
    [Dependency] private IRobustSerializer _serializer = default!;
    [Dependency] private IDependencyCollection _dependency = default!;
    [Dependency] private IResourceManager _resourceManager = default!;
    [Dependency] private MatchManagerSystem _match = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        _net.RegisterNetMessage<SetDebugInfo>();
    }

    // undo all the session stuff
    public void Serialize(EntityUid ent, string? type = "?")
    {
        var totalComps = AllEntityQuery<DebugMatchComponent>();
        var total = 0;
        foreach (var _ in totalComps)
        {
            total++;
        }

        var comp = EnsureComp<DebugMatchComponent>(ent);
        comp.SerializationNumber++;
        if (comp.DebugNumber == -99)
        {
            comp.DebugNumber = Random.Shared.Next(0, 1000000);

            // todo: make method to send message to all players in match
            foreach (var player in _match.GetPlayers(ent))
            {
                var matchStatus = new SetDebugInfo
                {
                    DebugInfoNumber =  comp.DebugNumber,
                };

                player.Channel.SendMessage(matchStatus);
            }
        }



        var path = new ResPath($"/DebugOutput/{comp.DebugNumber}/{comp.SerializationNumber} - {Comp<RoundComponent>(ent).RoundNumber} - {type}.txt");

        _resourceManager.UserData.CreateDir(path.Directory);
        var filestream = _resourceManager.UserData.OpenWriteText(path);
        WriteText(ent, filestream);

        // var serializer = new EntitySerializer(_dependency, SerializationOptions.Default);
        // // todo: this will not seralize player sessions, need to make own type for player sessions that is just their ID or something.
        // serializer.SerializeEntity(ent);
        //
        // var output = serializer.Write();
        //
        // var path = new ResPath($"/DebugOutput/{comp.DebugNumber}/{roundComp.RoundNumber}.yml");
        //
        // _resourceManager.UserData.CreateDir(path.Directory);
        // var filestream = _resourceManager.UserData.OpenWriteText(path);
        //
        // var document = new YamlDocument(output.ToYaml());
        // var stream = new YamlStream {document};
        // stream.Save(new YamlMappingFix(new Emitter(filestream)), false);
        //
        // filestream.Flush();
    }

    private void WriteText(EntityUid ent, StreamWriter fileStream)
    {
        fileStream.WriteLine(DateTime.UtcNow);

        if (TryComp<MatchComponent>(ent, out var match))
        {
            fileStream.WriteLine("MatchComponent:");
            fileStream.WriteLine($"  Current phase: {match.CurrentPhaseNumber}");
            fileStream.WriteLine($"  Current phaseType: {match.CurrentPhaseType}");
            fileStream.WriteLine($"  Players:");
            foreach (var player in match.Players)
            {
                fileStream.WriteLine($"  - {player.Name}");
            }

            fileStream.WriteLine($"  Phases:");
            foreach (var phases in match.MatchPhases)
            {
                fileStream.WriteLine($"  - {phases.Key}:");
                foreach (var phase in phases.Value)
                {
                    fileStream.WriteLine($"    - {phase.GetType()}:");
                }
            }
        }

        if (TryComp<RoundComponent>(ent, out var round))
        {
            fileStream.WriteLine("RoundComponent:");
            fileStream.WriteLine($"  Start time: {round.CurrentRoundStartTime}");
            fileStream.WriteLine($"  First to: {round.FirstToPoints}");
            fileStream.WriteLine($"  Max round time: {round.MaxRoundLength}");
            fileStream.WriteLine($"  Round number: {round.RoundNumber}");
            fileStream.WriteLine($"  Player to rounds won:");
            foreach (var player in round.TeamToRoundsWon)
            {
                fileStream.WriteLine($"  - {player.Key} : {player.Value}");
            }
        }

        // todo: add teams

        if (TryComp<MatchTeamComponent>(ent, out var matchTeamComp))
        {
            fileStream.WriteLine("MatchTeamComponent:");
            foreach (var team in matchTeamComp.Teams)
            {
                fileStream.WriteLine($"  {team.Key}:");
                foreach (var player in team.Value)
                {
                    fileStream.WriteLine($"   - {player.Name}");
                }
            }
        }

        if (TryComp<MatchKitComponent>(ent, out var kitComp))
        {
            fileStream.WriteLine("MatchKitComponent:");
            fileStream.WriteLine($"  Max Selection time: {kitComp.MaximumSelectionTime}");
            fileStream.WriteLine($"  Stop waiting time: {kitComp.StopWaitingKitTime}");
            fileStream.WriteLine($"  Available kits:");
            foreach (var playerToKits in kitComp.AvailableKits)
            {
                fileStream.WriteLine($"  {playerToKits.Key.Name}:");
                var x = 0;
                foreach (var kit in playerToKits.Value)
                {
                    fileStream.WriteLine($"  - {x}: ");
                    foreach (var section in kit)
                    {
                        fileStream.WriteLine($"    - {section.Key}: ");
                        foreach (var item in section.Value)
                        {
                            fileStream.WriteLine($"      - {item.Id}");
                        }
                    }
                    x++;
                }
            }

            fileStream.WriteLine($"  Chosen kits:");
            foreach (var playerToKits in kitComp.ChosenKits)
            {
                fileStream.WriteLine($"  {playerToKits.Key.Name} : {playerToKits.Value}");
            }
        }

        if (TryComp<MatchPlayerBodyComponent>(ent, out var body))
        {
            fileStream.WriteLine("MatchPlayerBodyComponent:");
            fileStream.WriteLine($"  Old Bodies:");
            foreach (var oldBody in body.OldBodies)
            {
                fileStream.WriteLine($"  - {oldBody.Key.Name} : {oldBody.Value.Id}");
            }
        }

        fileStream.Flush();
    }
}

