using System.Threading.Tasks;

namespace Werefox.Interfaces
{
    public interface ISendMessage
    {
        Task SendMessageAsync(string message);
    }
}