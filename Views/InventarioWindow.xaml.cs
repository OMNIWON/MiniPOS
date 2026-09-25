using System.Windows;
using POS.ViewModels;

namespace POS.Views;

public partial class InventarioWindow : Window
{
    public InventarioWindow(InventarioViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void RegresarAlMenu_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}