
using System.Windows;
using POS.ViewModels;

namespace POS.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.IniciarSesionExitoso += AlIniciarSesionExitoso;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InicializarAsync();
    }

    private async void Entrar_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.IniciarSesionCommand.ExecuteAsync(
            CajaContrasena.Password);
    }

    private void AlIniciarSesionExitoso()
    {
        DialogResult = true;
    }
}
