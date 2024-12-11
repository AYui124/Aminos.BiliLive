using Aminos.BiliLive.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Aminos.BiliLive.Views;

public partial class Login : UserControl
{
    public Login()
    {
        InitializeComponent();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
        {
            vm.OnViewShowCommand.Execute(null);
        }
    }
}