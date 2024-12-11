using SukiUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aminos.BiliLive.Models;
using Avalonia.Styling;
using SukiUI.Enums;
using Aminos.BiliLive.Utils;

namespace Aminos.BiliLive.Services
{
    public class ConfigService : ISingtonService
    {
        private Config _config = Config.Default;
        private readonly SukiTheme _theme = SukiTheme.GetInstance();
        private readonly string _configFileName =
            Path.Combine(PathTool.DocumentPath, "Aminos", "theme.json");

        public async Task ChangeBaseThemeAsync()
        {
            _theme.SwitchBaseTheme();
            var theme = _theme.ActiveBaseTheme;
            if (theme == ThemeVariant.Light)
            {
                _config.BaseTheme = 1;
            }
            else if (theme == ThemeVariant.Dark)
            {
                _config.BaseTheme = 2;
            }
            await SaveAsync();
        }

        public async Task ChangeThemeColorAsync()
        {
            _theme.SwitchColorTheme();
            var color = _theme.ActiveColorTheme?.DisplayName ?? "Red";
            _config.ThemeColor = color;
            await SaveAsync();
        }

        public async Task LoadAsync()
        {
            if (File.Exists(_configFileName))
            {
                await using var fs = File.OpenRead(_configFileName);
                var config = await JsonSerializer.DeserializeAsync<Config>(fs);
                if (config != null)
                {
                    _config = config;
                }
            }
            
            switch (_config.BaseTheme)
            {
                case 1:
                    _theme.ChangeBaseTheme(ThemeVariant.Light);
                    break;
                case 2:
                    _theme.ChangeBaseTheme(ThemeVariant.Dark);
                    break;
                default:
                    _theme.ChangeBaseTheme(ThemeVariant.Default);
                    break;
            }
            if (!Enum.TryParse(_config.ThemeColor, out SukiColor color))
            {
                color = SukiColor.Red;
            }
            _theme.ChangeColorTheme(color);
        }

        public async Task SaveAsync()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configFileName)!);
            await using var fs = File.Create(_configFileName);
            await JsonSerializer.SerializeAsync(fs, _config);
        }
    }
}
