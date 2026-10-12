using System.Globalization;
using System.Windows;

namespace POS.Views;

public partial class CantidadWindow : Window
{
    private readonly bool _permiteFraccion;

    public decimal Cantidad { get; private set; }

    public CantidadWindow(string titulo, string pregunta, bool permiteFraccion)
    {
        InitializeComponent();
        Title = titulo;
        TextoPregunta.Text = pregunta;
        _permiteFraccion = permiteFraccion;

        Loaded += (s, e) =>
        {
            CajaCantidad.Focus();
            CajaCantidad.SelectAll();
        };
    }

    private void Aceptar_Click(object sender, RoutedEventArgs e)
    {
        string texto = CajaCantidad.Text.Trim();

        if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valor) &&
            !decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor))
        {
            MostrarError("Escribe un número válido.");
            return;
        }

        if (valor <= 0)
        {
            MostrarError("La cantidad debe ser mayor a cero.");
            return;
        }

        if (!_permiteFraccion && valor != Math.Floor(valor))
        {
            MostrarError("Este producto solo se maneja por pieza completa.");
            return;
        }

        Cantidad = valor;
        DialogResult = true;
    }

    private void MostrarError(string mensaje)
    {
        TextoError.Text = mensaje;
        TextoError.Visibility = Visibility.Visible;
    }
}