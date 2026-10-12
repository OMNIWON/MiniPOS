using System.Windows;
using POS.Services;
using POS.ViewModels;

namespace POS.Views;

public partial class CobroWindow : Window
{
    public CobroViewModel ViewModel { get; }

    public CobroWindow(DesgloseVenta desglose)
    {
        InitializeComponent();
        ViewModel = new CobroViewModel(desglose);
        DataContext = ViewModel;
    }

    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.PuedeConfirmar) return;
        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}