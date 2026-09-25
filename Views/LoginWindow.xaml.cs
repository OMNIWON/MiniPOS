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
        DataContext = viewModel;

        _viewModel.IniciarSesionExitoso += () =>
        {
            DialogResult = true;
            Close();
        };
    }

    private async void Entrar_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.IniciarSesionCommand.ExecuteAsync(CajaContrasena.Password);
    }
}