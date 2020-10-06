using Microsoft.CSharp.RuntimeBinder;
using System.Threading.Tasks;

namespace FileEvents
{
    public class AddCommandHandler : ICommandHandler
    {
        private const string Type = "Add";
        private readonly ISourceControl _sourceControl;

        public AddCommandHandler(ISourceControl sourceControl)
        {
            _sourceControl = sourceControl;
        }

        public bool CanHandle(dynamic command)
        {
            try
            {
                return string.Equals((string)command.Type, Type) &&
                       !string.IsNullOrWhiteSpace((string)command.Path);
            }
            catch (RuntimeBinderException)
            {
            }
            return false;
        }

        public async Task Execute(dynamic command)
        {
            await _sourceControl.Add((string)command.Path);
        }
    }
}
