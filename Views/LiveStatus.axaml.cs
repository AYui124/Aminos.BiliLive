using Aminos.BiliLive.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Aminos.BiliLive.Views;

public partial class LiveStatus : UserControl
{
    public LiveStatus()
    {
        InitializeComponent();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LiveStatusViewModel vm)
        {
            vm.OnViewShowCommand.Execute(null);
        }
    }
}