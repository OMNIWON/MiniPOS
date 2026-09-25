using System.Windows;
using POS.ViewModels;

namespace POS.Views;

public partial class HistorialWindow : Window
{
    public HistorialWindow(HistorialViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void RegresarAlMenu_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}