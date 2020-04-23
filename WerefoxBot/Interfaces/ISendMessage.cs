using System.Threading.Tasks;

namespace WerefoxBot.Interfaces
{
    public interface ISendMessage
    {
        Task SendMessageAsync(string message);
    }
}