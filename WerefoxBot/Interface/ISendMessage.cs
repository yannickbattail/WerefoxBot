using System.Threading.Tasks;

namespace WerefoxBot.Interface
{
    public interface ISendMessage
    {
        Task SendMessageAsync(string message);
    }
}