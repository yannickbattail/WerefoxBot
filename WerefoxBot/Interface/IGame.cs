using System.Collections.Generic;
using System.Threading.Tasks;

namespace WerefoxBot.Interface
{
    internal interface IGame : ISendMessage
    {
        IList<IPlayer> Players { get; set; }
        GameStep Step { get; set; }
        Task SendMessageAsync(string message);
        IEnumerable<IPlayer> GetAlivePlayers();
        IEnumerable<IPlayer> GetDeadPlayers();
        IEnumerable<IPlayer> GetAliveWerefoxes();
        IPlayer? GetByName(string? displayName);
        IPlayer? GetById(ulong? id);
    }
}