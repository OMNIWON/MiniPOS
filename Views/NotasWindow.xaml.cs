using System.Windows;

namespace POS.Views;

public partial class NotasWindow : Window
{
    public string? Nota1 { get; private set; }
    public string? Nota2 { get; private set; }

    public NotasWindow(string? nota1, string? nota2)
    {
        InitializeComponent();
        Nota1TextBox.Text = nota1 ?? string.Empty;
        Nota2TextBox.Text = nota2 ?? string.Empty;
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        Nota1 = Normalizar(Nota1TextBox.Text);
        Nota2 = Normalizar(Nota2TextBox.Text);
        DialogResult = true;
    }

    private static string? Normalizar(string texto)
    {
        texto = texto.TrimEnd();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}