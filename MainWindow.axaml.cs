using Aminos.BiliLive.Utils;
using Aminos.BiliLive.ViewModels;
using Avalonia.Interactivity;
using SukiUI.Controls;

namespace Aminos.BiliLive
{
    public partial class MainWindow: SukiWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.OnViewShowCommand.Execute(null);
            }
        }
    }
}