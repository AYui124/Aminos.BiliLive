using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Services
{
    public class ClipboardService : ISingtonService
    {
        private readonly IClassicDesktopStyleApplicationLifetime _desktopLifetime;
        public ClipboardService(IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            _desktopLifetime = desktopLifetime;
        }

        public async Task SetTextAsync(string text)
        {
            var window = _desktopLifetime.MainWindow;
            var clipboard = window?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(text);
            }
            else
            {
                throw new InvalidOperationException("Clipboard is not available.");
            }
        }

        public async Task<string?> GetTextAsync()
        {
            var window = _desktopLifetime.MainWindow;
            var clipboard = window?.Clipboard;
            if (clipboard != null)
            {
                return await clipboard.GetTextAsync();
            }
            else
            {
                return null; 
            }
        }
    }
}
