using System.Collections.Generic;

namespace Werefox.Interfaces
{
    public interface IGame : ISendMessage
    {
        IList<IPlayer> Players { get; set; }
        GameStep Step { get; set; }
        IEnumerable<IPlayer> GetAlivePlayers();
        IEnumerable<IPlayer> GetDeadPlayers();
        IEnumerable<IPlayer> GetAliveWerefoxes();
        IPlayer? GetByName(string? displayName);
        IPlayer? GetById(ulong? id);
    }
}