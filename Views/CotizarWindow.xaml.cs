using System.Windows;
using POS.ViewModels;

namespace POS.Views;

public partial class CotizarWindow : Window
{
    public CotizarWindow(CotizarViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void RegresarAlMenu_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}