using System.Windows;
using POS.ViewModels;

namespace POS.Views;

public partial class CobroWindow : Window
{
    public CobroViewModel ViewModel { get; }

    public CobroWindow(decimal total)
    {
        InitializeComponent();
        ViewModel = new CobroViewModel(total);
        DataContext = ViewModel;
    }

    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.PuedeConfirmar) return;
        DialogResult = true;
        Close();
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}