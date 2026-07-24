using Content.Shared.FeedbackSystem;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._ROBUST.Match;

public sealed class SharedMatchSystem
{

}

// [Serializable, NetSerializable, DataDefinition]
// public sealed partial class PlayerData()
// {
//     public PlayerData(ICommonSession session) : this()
//     {
//         NetUserId = session.UserId;
//     }
//
//     public NetUserId NetUserId;
// };

[Serializable, NetSerializable]
public sealed class RequestMatchStart : EntityEventArgs
{
    public string Username = "";
}

public sealed class ConfirmMatchStart : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
    }

    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableUnordered;
}


[Serializable, NetSerializable]
public struct MatchRoundStats
{
    public int RoundNumber;
    public int MaxRounds;
    public Dictionary<string, int> PlayerToRoundsWon;
    public TimeSpan MaxRoundLength;
}

[Serializable, NetSerializable]
public sealed class MatchRoundStartMessage : EntityEventArgs
{
    public MatchRoundStats stats;

    public MatchRoundStartMessage(MatchRoundStats stats)
    {
        this.stats = stats;
    }
}


[Serializable, NetSerializable]
public sealed class KitsToChooseFromMessage : EntityEventArgs
{
    public List<Dictionary<string, List<EntProtoId>>> Kits;

    public TimeSpan MaximumSelectionTime;

    public KitsToChooseFromMessage(List<Dictionary<string, List<EntProtoId>>> kits)
    {
        Kits = kits;
    }
}

public sealed class KitChosenMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public int Kit = 0;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Kit = buffer.ReadInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Kit);
    }

    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableUnordered;
}

public sealed class SetTimerScoreMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public TimeSpan TimerLength;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        TimerLength = buffer.ReadTimeSpan();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(TimerLength);
    }

    public override NetDeliveryMethod DeliveryMethod =>  NetDeliveryMethod.ReliableUnordered;
}

public sealed class CurrentScoreMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public int RoundNumber;

    public int FirstToPoints;

    // todo probably change this so you don't have to know what team your on to get the message.
    public string YourTeam = "";

    // player GUID -> score
    public Dictionary<string, int> Scores = new();

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        RoundNumber = buffer.ReadInt32();
        FirstToPoints = buffer.ReadInt32();
        YourTeam = buffer.ReadString();

        var count = buffer.ReadInt32();

        for (var i = 0; i < count; i++)
        {
            Scores.Add(buffer.ReadString(), buffer.ReadInt32());
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(RoundNumber);
        buffer.Write(FirstToPoints);
        buffer.Write(YourTeam);

        buffer.Write(Scores.Count);

        foreach (var (teamName, score) in Scores)
        {
            buffer.Write(teamName);
            buffer.Write(score);
        }
    }

    public override NetDeliveryMethod DeliveryMethod =>  NetDeliveryMethod.ReliableUnordered;
}

public sealed class HideScoreboard : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer){}

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer){}

    public override NetDeliveryMethod DeliveryMethod =>  NetDeliveryMethod.ReliableUnordered;
}

public sealed class SetDebugInfo : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public int DebugInfoNumber = 0;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        DebugInfoNumber = buffer.ReadInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(DebugInfoNumber);
    }

    public override NetDeliveryMethod DeliveryMethod =>  NetDeliveryMethod.ReliableUnordered;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MatchSettings
{
    [DataField]
    public Dictionary<string, string> Settings = new();
}
