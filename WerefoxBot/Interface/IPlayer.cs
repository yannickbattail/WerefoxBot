using System;
using System.Threading.Tasks;

namespace WerefoxBot.Interface
{
    internal interface IPlayer : ISendMessage
    {
        IPlayer? Vote { get; set; }
        Card Card { get; set; }
        bool IsLover { get; set; }
        PlayerState State { get; set; }
        Task SendMessageAsync(string message);
        ulong GetId();
        String GetMention();
        String GetDisplayName();
    }
}