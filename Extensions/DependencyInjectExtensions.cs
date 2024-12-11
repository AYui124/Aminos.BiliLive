using Aminos.BiliLive.Services;
using Aminos.BiliLive.ViewModels;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aminos.BiliLive.Extensions
{
    public static class DependencyInjectExtensions
    {
        public static IServiceCollection UseInject(this IClassicDesktopStyleApplicationLifetime desktop)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(desktop);
            return serviceCollection;
        }

        public static IServiceCollection InjectViewModels(this IServiceCollection services)
        {
            // ViewModel可注册成瞬态：
            // 因项目加载顺序，所有ViewModelBase都在MainWindowViewModel中实例化，
            // 生命周期与MainWindowViewModel相同
            // MainWindowViewModel直到主窗体关闭才释放
            // 基本可认为虽然注册为瞬态，但实际生命周期与单例相同
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(o => typeof(ViewModelBase).IsAssignableFrom(o) && !o.IsAbstract);
            foreach (var type in types) 
            {
                services.AddTransient(typeof(ViewModelBase), type);
            }
            services.AddTransient<MainWindowViewModel>();
            return services;
        }

        public static IServiceCollection InjectServices(this IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(o => typeof(ISingtonService).IsAssignableFrom(o) && !o.IsAbstract);
            foreach (var type in types)
            {
                services.AddSingleton(type);
            }
            services.AddSingleton<ISukiToastManager, SukiToastManager>();
            services.AddSingleton<ISukiDialogManager, SukiDialogManager>();
            services.AddHttpClient();
            return services;
        }
    }
}
