using Aminos.BiliLive.Models;
using Aminos.BiliLive.Services;
using Aminos.BiliLive.Utils;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HarfBuzzSharp;
using Material.Icons;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aminos.BiliLive.ViewModels
{
    public partial class LoginViewModel : ViewModelBase
    {
        #region RouteInfo
        public override MaterialIconKind Icon => MaterialIconKind.User;

        protected override string ViewName => "Login";

        public override string MenuName => "登录";

        public override int Index => 1;

        public override string ModelName => nameof(LoginViewModel);
        #endregion

        [ObservableProperty]
        private LoginState _status;

        [ObservableProperty]
        private bool _loading;

        [ObservableProperty]
        private IImage? _qrImageBuffer;

        [ObservableProperty]
        private IImage? _avatarImage;

        [ObservableProperty]
        private string? _qrTips;

        [ObservableProperty]
        private MemberInfo _member = MemberInfo.Anonymous;

        private bool _hasLoad;
        private QrLoginInfo? _qrInfo;
        private readonly DispatcherTimer _timer;
        private int _timeLock;
        private TimeSpan _timerInterval = TimeSpan.FromSeconds(2);

        public IRelayCommand OnViewShowCommand => new RelayCommand(OnViewShow);

        private readonly UserDataService _userDataService;

        public LoginViewModel(UserDataService userDataService)
        {
            _userDataService = userDataService;
            _timer = new DispatcherTimer(_timerInterval, DispatcherPriority.Normal, QueryQrStatus)
            {
                IsEnabled = false
            };
        }

        private async void OnViewShow()
        {
            if (_hasLoad)
            {
                return;
            }
            _hasLoad = true;
            await OnLoad();
        }

        private async Task OnLoad()
        {
            SwitchToStep(LoginState.Preparing);

            Loading = true;
            await Task.Delay(100);

            await _userDataService.LoadAsync();
            if (_userDataService.HasLoginBefore())
            {
                if (!_userDataService.IsValidUser())
                {
                    // 刷新验证cookie
                    await RefreshCookiesAsync();
                }
                else
                {
                    // 开始登陆流程
                    await ShowQrLoginAsync();
                }
            }
            else
            {
                // 开始登陆流程
                await ShowQrLoginAsync();
            }
            Loading = false;
        }

        private async Task RefreshCookiesAsync()
        {
            if (_userDataService.HasCheckedRecently())
            {
                _userDataService.SetUserValidated();
                SwitchToStep(LoginState.Success);
                return;
            }

            var result = await _userDataService.RefreshCookiesAsync();
            if (result.Success)
            {
                SwitchToStep(LoginState.Success);
                return;
            }
            await ShowQrLoginAsync();
        }

        private async Task ShowQrLoginAsync()
        {
            // 获取二维码url及key
            var infoResult = await _userDataService.GetQrLoginAsync();
            //await Task.Delay(2000);
            //var infoResult = BizResult<QrLoginInfo>.AsSuccess(new QrLoginInfo { Url = "http://11111", QrKey = "1223" });
            if (!infoResult.Success)
            {
                SwitchToStep(LoginState.Error);
                return;
            }
            _qrInfo = infoResult.Data;
            if (string.IsNullOrEmpty(_qrInfo?.Url) || string.IsNullOrEmpty(_qrInfo?.QrKey))
            {
                SwitchToStep(LoginState.Error);
                return;
            }
            var buffer = QrImageTool.GenerateQrImage(_qrInfo.Url);
            using var ms = new MemoryStream(buffer);
            QrImageBuffer = new Avalonia.Media.Imaging.Bitmap(ms);
            SwitchToStep(LoginState.QrImage);
            StartQrStatusTimer();
        }

        private async void QueryQrStatus(object? sender, EventArgs e)
        {
            if (Interlocked.Exchange(ref _timeLock, 1) == 0)
            {
                if (string.IsNullOrEmpty(_qrInfo?.QrKey))
                {
                    Interlocked.Exchange(ref _timeLock, 0);
                    return;
                }
                var queryResult = await _userDataService.GetQrStatusAsync(_qrInfo.QrKey);
                if (queryResult is { Success: true, Data: not null })
                {
                    if (queryResult.Data.Status == QrStatus.Success)
                    {
                        await _userDataService.BuildUserInfoAsync(queryResult.Data);
                        SwitchToStep(LoginState.Success);
                        StopQrStatusTimer();
                    }
                    if (queryResult.Data.Status == QrStatus.OutofDate)
                    {
                        SwitchToStep(LoginState.Error);
                        QrTips = "二维码已过期，请点击刷新重试！";
                        StopQrStatusTimer();
                    }
                    if (queryResult.Data.Status == QrStatus.Scanned)
                    {
                        QrTips = "已扫码待确认";
                        var random = Random.Shared.Next(500, 1500);
                        _timerInterval = TimeSpan.FromMilliseconds(random);
                    }
                    if (queryResult.Data.Status == QrStatus.NotScanned)
                    {
                        var random = Random.Shared.Next(1500, 2500);
                        _timerInterval = TimeSpan.FromMilliseconds(random);
                    }
                }
                Interlocked.Exchange(ref _timeLock, 0);
            }
        }

        async partial void OnStatusChanged(LoginState oldValue, LoginState newValue)
        {
            if (newValue == LoginState.Success)
            {
                Loading = true;
                var result = await _userDataService.GetMemberInfoAsync();
                if (result is { Success: true, Data: not null })
                {
                    Member = result.Data;
                    var faceBuffer = _userDataService.GetCachedAvatar();
                    if (faceBuffer is { Length: > 0 })
                    {
                        using var ms = new MemoryStream(faceBuffer);
                        AvatarImage = new Avalonia.Media.Imaging.Bitmap(ms);
                    }
                    else
                    {
                        var face = await _userDataService.GetAvatarAsync(Member.FaceUrl);
                        if (face is { Success: true, Data: not null })
                        {
                            await _userDataService.SaveAvatarAsync(face.Data);
                            using var ms = new MemoryStream(face.Data);
                            AvatarImage = new Avalonia.Media.Imaging.Bitmap(ms);
                        }
                    }
                    
                }
                Loading = false;
            }
        }

        private void StartQrStatusTimer()
        {
            _timer.IsEnabled = true;
            _timer.Start();
        }

        private void StopQrStatusTimer()
        {
            _timer.IsEnabled = false;
            _timer.Stop();
        }

        private void SwitchToStep(LoginState value)
        {
            Status = value;
        }

        [RelayCommand]
        public async Task RetryLoginCommand()
        {
            _timer.IsEnabled = false;
            _timer.Stop();
            Loading = true;
            await ShowQrLoginAsync();
            Loading = false;
        }
    }
}
