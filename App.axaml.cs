using Aminos.BiliLive.Extensions;
using Aminos.BiliLive.Services;
using Aminos.BiliLive.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;

namespace Aminos.BiliLive
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var splash = new SplashWindow();
                splash.Show();
                await Task.Delay(100);
                var sc = desktop.UseInject();
                sc.InjectServices();
                sc.InjectViewModels();
                // Other dependency inject action
                var sp = sc.BuildServiceProvider();
                var mainWindowViewModel = sp.GetRequiredService<MainWindowViewModel>();
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };
                desktop.MainWindow.Closed += (_, __) => splash.Close();
                desktop.MainWindow.Show();
                splash.Close();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins

            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}