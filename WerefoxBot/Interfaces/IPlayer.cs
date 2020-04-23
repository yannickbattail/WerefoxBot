using System;

namespace WerefoxBot.Interfaces
{
    public interface IPlayer : ISendMessage
    {
        IPlayer? Vote { get; set; }
        Card Card { get; set; }
        bool IsLover { get; set; }
        PlayerState State { get; set; }
        ulong GetId();
        String GetMention();
        String GetDisplayName();
    }
}