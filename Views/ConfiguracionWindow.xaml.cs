using System.Windows;
using POS.ViewModels;

namespace POS.Views;

public partial class ConfiguracionWindow : Window
{
    public ConfiguracionWindow(ConfiguracionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Guardado += () => DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}