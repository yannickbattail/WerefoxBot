namespace Werefox.Interfaces
{
    public interface IPlayer : ISendMessage
    {
        IPlayer? Vote { get; set; }
        Card Card { get; set; }
        bool IsLover { get; set; }
        PlayerState State { get; set; }
        string GetId();
        string GetMention();
        string GetDisplayName();
    }
}