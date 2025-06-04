using Aminos.BiliLive.Models;
using Aminos.BiliLive.Services;
using Aminos.BiliLive.Utils;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Toasts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Aminos.BiliLive.ViewModels
{
    public partial class LiveStatusViewModel : ViewModelBase
    {
        #region RouteInfo
        public override MaterialIconKind Icon => MaterialIconKind.World;
        
        protected override string ViewName => "LiveStatus";

        public override string MenuName => "直播";

        public override int Index => 2;

        public override string ModelName => nameof(LiveStatusViewModel);
        #endregion

        [ObservableProperty]
        private bool _loading;

        [ObservableProperty]
        private LivingInfo _livingInfo = LivingInfo.Default;

        public AvaloniaList<LabelValueOption> ParentAreas { get; } = new();
        public AvaloniaList<LabelValueOption> SubAreas { get; } = new();

        [ObservableProperty]
        private LabelValueOption _parentArea = new();

        [ObservableProperty]
        private LabelValueOption _subArea = new();

        [ObservableProperty]
        private bool _startLiveLoading;

        [ObservableProperty]
        private bool _stopLiveLoading;

        [ObservableProperty]
        private RtmpInfo _rtmpServer = RtmpInfo.Default;

        private bool _hasLoad;

        public IRelayCommand OnViewShowCommand => new RelayCommand(OnViewShow);

        private readonly UserDataService _userDataService;
        private readonly ManageLiveService _manageLiveService;
        private readonly ClipboardService _clipboardService;
        private readonly LiveAreaService _liveAreaService;
        private readonly ISukiToastManager _toastManager;
        

        public LiveStatusViewModel(
            UserDataService userDataService,
            ManageLiveService manageLiveService,
            ClipboardService clipboardService,
            LiveAreaService liveAreaService, 
            ISukiToastManager toastManager)
        {
            _userDataService = userDataService;
            _manageLiveService = manageLiveService;
            _toastManager = toastManager;
            _liveAreaService = liveAreaService;
            _clipboardService = clipboardService;
        }

        private async void OnViewShow()
        {
            Loading = true;
            await CheckLogin();
            if (_hasLoad)
            {
                Loading = false;
                return;
            }
            _hasLoad = true;
            await OnLoad();
            Loading = false;
        }

        private async Task OnLoad()
        {
            await Task.Delay(100);
            await _liveAreaService.LoadAsync();

            await GetLiveStatusAsync();
            BindParentAreas();
            ChangeParentArea();
        }

        private async Task GetLiveStatusAsync()
        {
            var userId = _userDataService.GetUserId();
            var result = await _manageLiveService.GetStatusAsync(userId);
            if (result is { Success: true, Data: not null })
            {
                LivingInfo = result.Data;
            }
        }

        private void BindParentAreas()
        {
            var list = _liveAreaService.GetAreas();
            ParentAreas.Clear();
            ParentAreas.AddRange(list.Select(o => new LabelValueOption { Label = o.Name, Value = o.Id.ToString() }));
        }

        private void ChangeParentArea()
        {
            ParentArea = ParentAreas
                             .FirstOrDefault(o => o.Value == LivingInfo.ParentArea.ToString())
                         ?? ParentAreas.First();
        }

        [RelayCommand]
        public async Task RefreshStatus()
        {
            await GetLiveStatusAsync();
            ChangeParentArea();
            ChangeSubArea();
        }

        [RelayCommand]
        public async Task StartLiveCommand()
        {
            StartLiveLoading = true;
            var area = SubArea.Value;
            var result = await _manageLiveService.StartLiveAsync(area);
            if (result is { Success: true, Data: not null })
            {
                RtmpServer = result.Data;
                await GetLiveStatusAsync();
            }
            else
            {
                _toastManager
                .CreateToast()
                .OfType(Avalonia.Controls.Notifications.NotificationType.Success)
                .WithTitle("提示")
                .WithContent("失败：" + result.Message)
                .Dismiss()
                .After(TimeSpan.FromSeconds(3))
                .Queue();
            }
            StartLiveLoading = false;
        }

        [RelayCommand]
        public async Task EndLiveCommand()
        {
            StopLiveLoading = true;
            await _manageLiveService.StopLiveAsync();
            await GetLiveStatusAsync();
            RtmpServer = RtmpInfo.Default;
            StopLiveLoading = false;
        }

        [RelayCommand]
        public async Task CopyUrlToClipboard()
        {
            await _clipboardService.SetTextAsync(RtmpServer.RtmpUrl);
            _toastManager
                .CreateToast()
                .OfType(Avalonia.Controls.Notifications.NotificationType.Success)
                .WithTitle("提示")
                .WithContent("已复制!")
                .Dismiss()
                .After(TimeSpan.FromSeconds(2))
                .Queue();
        }

        [RelayCommand]
        public async Task CopyKeyToClipboard()
        {
            await _clipboardService.SetTextAsync(RtmpServer.RtmpKey);
            _toastManager
                .CreateToast()
                .OfType(Avalonia.Controls.Notifications.NotificationType.Success)
                .WithTitle("提示")
                .WithContent("已复制!")
                .Dismiss()
                .After(TimeSpan.FromSeconds(2))
                .Queue();
        }

        [RelayCommand]
        public async Task ChangeRoomName()
        {
            await _manageLiveService.SetRoomTitleAsync(LivingInfo.RoomName);
            _toastManager
                .CreateToast()
                .OfType(Avalonia.Controls.Notifications.NotificationType.Success)
                .WithTitle("提示")
                .WithContent("已修改直播间名!")
                .Dismiss()
                .After(TimeSpan.FromSeconds(1))
                .Queue();
        }

        partial void OnParentAreaChanged(LabelValueOption? oldValue, LabelValueOption newValue)
        {
            var list = _liveAreaService.GetAreas();
            var id = int.Parse(newValue.Value);
            var parent = list.FirstOrDefault(o => o.Id == id);
            if (parent != null)
            {
                SubAreas.Clear();
                SubAreas.AddRange(parent.Areas.Select(o => new LabelValueOption { Label = o.Name, Value = o.Id.ToString() }));
                
                ChangeSubArea();
            }
            else
            {
                _toastManager
                    .CreateToast()
                    .WithTitle("提示")
                    .WithContent("不存在这个分区，请清除缓存再试！")
                    .Dismiss()
                    .After(TimeSpan.FromSeconds(2))
                    .Queue();

                ParentArea = oldValue ?? ParentAreas.First();
            }
        }

        private void ChangeSubArea()
        {
            SubArea = SubAreas
                          .FirstOrDefault(o => o.Value == LivingInfo.LastLiveArea.ToString())
                      ?? SubAreas.First();
        }

        private async Task InitRoomIdAsync()
        {
            var roomId = _userDataService.GetRoomId();
            if (string.IsNullOrEmpty(roomId))
            {
                var userId = _userDataService.GetUserId();
                var result = await _manageLiveService.GetRoomIdAsync(userId);
                if (result.Success && !string.IsNullOrEmpty(result.Data))
                {
                    await _userDataService.SaveRoomIdAsync(result.Data);
                }
            }
        }

        private async Task CheckLogin()
        {
            if (!_userDataService.IsValidUser())
            {
                _toastManager
                    .CreateToast()
                    .WithTitle("提示")
                    .WithContent("无法获取用户信息，请先完成登录！")
                    .Dismiss()
                    .After(TimeSpan.FromSeconds(2))
                    .Queue();
                await Task.Delay(1000);
                MinimalEventBus.Global.Publish(MinimalEventBus.EventName.ChangeMenu, new MinimalEventArg { Data = "LoginViewModel" });
            }
            else
            {
                await InitRoomIdAsync();
            }
        }
    }
}
