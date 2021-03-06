﻿using Microsoft.CSharp.RuntimeBinder;
using System.Threading.Tasks;

namespace FileEvents
{
    public class PreviewCommandHandler : ICommandHandler
    {
        private const string Type = "Preview";
        private readonly ISourceControl _sourceControl;

        public PreviewCommandHandler(ISourceControl sourceControl)
        {
            _sourceControl = sourceControl;
        }

        public bool CanHandle(dynamic command)
        {
            try
            {
                return string.Equals((string)command.Type, Type) &&
                       !string.IsNullOrWhiteSpace((string)command.Path) &&
                       !string.IsNullOrWhiteSpace((string)command.Destination) &&
                       int.TryParse((string)command.Bookmark, out var _);
            }
            catch (RuntimeBinderException)
            {
            }
            return false;
        }

        public async Task Execute(dynamic command)
        {
            var document = await _sourceControl.Preview((string)command.Path, int.Parse((string)command.Bookmark));
            await document.Save((string)command.Destination);
        }
    }
}
