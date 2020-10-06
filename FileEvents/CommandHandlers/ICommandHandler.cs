using System.Threading.Tasks;

namespace FileEvents
{
    public interface ICommandHandler
    {
        bool CanHandle(dynamic command);
        Task Execute(dynamic command);
    }
}
