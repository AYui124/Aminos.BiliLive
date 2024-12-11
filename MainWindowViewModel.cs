using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aminos.BiliLive.Models;
using Aminos.BiliLive.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using SukiUI;
using Aminos.BiliLive.Services;
using CommunityToolkit.Mvvm.Messaging;
using Tmds.DBus.Protocol;
using Aminos.BiliLive.Utils;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace Aminos.BiliLive
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public AvaloniaList<ViewModelBase> Pages { get; }

        [ObservableProperty]
        private ViewModelBase? _currentPage;

        [ObservableProperty]
        private bool _loading;

        private readonly ConfigService _configService;
        private readonly IServiceProvider _serviceProvider;
        public ISukiToastManager ToastManager { get; }
        public ISukiDialogManager DialogManager { get; }

        public MainWindowViewModel(
            ConfigService configService,
            IServiceProvider serviceProvider,
            ISukiToastManager toastManager,
            ISukiDialogManager dialogManager)
        {
            _configService = configService;
            _serviceProvider = serviceProvider;
            ToastManager = toastManager;
            DialogManager = dialogManager;
            Pages = new AvaloniaList<ViewModelBase>();

            MinimalEventBus.Global.Subscribe(MinimalEventBus.EventName.ChangeMenu, HandleChangeMenu);
            MinimalEventBus.Global.Subscribe(MinimalEventBus.EventName.ReloadMenu, HandleReloadMenu);
        }

        ~MainWindowViewModel()
        {
            MinimalEventBus.Global.Unsubscribe(MinimalEventBus.EventName.ChangeMenu, HandleChangeMenu);
            MinimalEventBus.Global.Unsubscribe(MinimalEventBus.EventName.ReloadMenu, HandleReloadMenu);
        }

        public IRelayCommand OnViewShowCommand => new RelayCommand(OnViewShow);
        
        private async void OnViewShow()
        {
            Loading = true;
            await _configService.LoadAsync();
            await Task.Delay(500);
            var vms = _serviceProvider.GetServices<ViewModelBase>();
            Loading = false;
            Pages.AddRange(vms.OrderBy(o => o.Index));
        }

        private void HandleChangeMenu(MinimalEventArg eventArg)
        {
            Loading = true;
            if (eventArg.Data is string modelName)
            {
                var find = Pages.FirstOrDefault(o => o.ModelName == modelName);
                if (find != null)
                {
                    CurrentPage = find;
                }
            }
            Loading = false;
        }

        private async void HandleReloadMenu(MinimalEventArg eventArg)
        {
            Loading = true;
            Pages.Clear();
            await Task.Delay(500);
            var vms = _serviceProvider.GetServices<ViewModelBase>();
            Pages.AddRange(vms.OrderBy(o => o.Index));
            Loading = false;
        }

        [RelayCommand]
        public async Task ChangeLightDarkCommand()
        {
            await _configService.ChangeBaseThemeAsync();
        }

        [RelayCommand]
        public async Task ChangeThemeColorCommand()
        {
            await _configService.ChangeThemeColorAsync();
        }
    }
}
