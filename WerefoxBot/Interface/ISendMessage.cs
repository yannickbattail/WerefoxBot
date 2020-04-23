using System.Threading.Tasks;

namespace WerefoxBot.Interface
{
    internal interface ISendMessage
    {
        Task SendMessageAsync(string message);
    }
}