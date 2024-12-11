using Aminos.BiliLive.Models;
using Aminos.BiliLive.Services;
using Aminos.BiliLive.Utils;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.ViewModels
{
    public partial class SettingViewModel : ViewModelBase
    {
        #region RouteInfo
        public override MaterialIconKind Icon => MaterialIconKind.Settings;

        protected override string ViewName => "Setting";

        public override string MenuName => "设置";

        public override int Index => 3;

        public override string ModelName => nameof(SettingViewModel);
        #endregion

        [ObservableProperty]
        private bool _loading;

        public string Version => Program.Version;

        private readonly LiveAreaService _liveAreaService;
        private readonly UserDataService _userDataService;

        public SettingViewModel(LiveAreaService liveAreaService,
            UserDataService userDataService)
        {
            _liveAreaService = liveAreaService;
            _userDataService = userDataService;
        }

        [RelayCommand]
        public async Task ClearUserCache()
        {
            await _userDataService.ClearAsync();
            MinimalEventBus.Global.Publish(MinimalEventBus.EventName.ReloadMenu, new MinimalEventArg());
        }

        [RelayCommand]
        public async Task ClearAreaCache()
        {
            await _liveAreaService.ClearAsync();
            MinimalEventBus.Global.Publish(MinimalEventBus.EventName.ReloadMenu, new MinimalEventArg());
        }

        [RelayCommand]
        public void GotoUrl()
        {
            var url = "https://github.com/AYui124/Aminos.BiliLive";
            LinkTool.OpenUrl(url);
        }
    }
}
