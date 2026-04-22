using SistemaInventario.Models;
using SistemaInventario.Services;
using System.Globalization;
using System.Windows.Controls;

using System.Windows;

using SistemaInventario.Models;
using SistemaInventario.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SistemaInventario.Views
{
    public partial class InventarioPage : Page
    {
        private readonly ProductoService _productoService = new();
        private List<Producto> _productos = new();
        private List<Categoria> _categorias = new();
        private Producto? _productoSeleccionado;

        public InventarioPage()
        {
            InitializeComponent();
            CargarCategorias();
            CargarInventario();
        }

        private void CargarCategorias()
        {
            _categorias = _productoService.ObtenerCategorias();

            CbCategoriaFiltro.ItemsSource = null;
            CbCategoriaFiltro.ItemsSource = _categorias;
            CbCategoriaFiltro.SelectedItem = null;
        }

        private void CargarInventario()
        {
            _productos = _productoService.ObtenerTodos()
                .OrderBy(p => p.Nombre)
                .ToList();

            AplicarFiltros();
            ActualizarResumen();
        }

        private void AplicarFiltros()
        {
            string texto = TxtBuscar.Text.Trim().ToLower();
            int? categoriaId = CbCategoriaFiltro.SelectedValue as int?;

            var filtrados = _productos.Where(p =>
                (string.IsNullOrWhiteSpace(texto) ||
                 p.Nombre.ToLower().Contains(texto) ||
                 (p.Categoria != null && p.Categoria.Nombre.ToLower().Contains(texto)))
                &&
                (!categoriaId.HasValue || p.CategoriaId == categoriaId.Value)
            ).ToList();

            DgInventario.ItemsSource = null;
            DgInventario.ItemsSource = filtrados;
        }

        private void ActualizarResumen()
        {
            TxtResumenProductos.Text = _productos.Count.ToString();
            TxtResumenStockBajo.Text = _productos.Count(p => p.Stock <= 5).ToString();
            TxtResumenCategorias.Text = _productos.Select(p => p.CategoriaId).Distinct().Count().ToString();
        }

        private void DgInventario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgInventario.SelectedItem is not Producto producto)
                return;

            _productoSeleccionado = producto;
            TxtProductoSeleccionado.Text = producto.Nombre;
            TxtStockActual.Text = producto.Stock.ToString();
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void CbCategoriaFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnEntrada_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtCantidadAjuste.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int cantidad))
            {
                if (!int.TryParse(TxtCantidadAjuste.Text, out cantidad))
                {
                    MessageBox.Show("Cantidad inválida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _productoService.AjustarStock(_productoSeleccionado.Id, cantidad);
            CargarInventario();
            RefrescarSeleccion();
            TxtCantidadAjuste.Text = string.Empty;

            MessageBox.Show("Entrada de stock registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnSalida_Click(object sender, RoutedEventArgs e)
        {
            if (_productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtCantidadAjuste.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out int cantidad))
            {
                if (!int.TryParse(TxtCantidadAjuste.Text, out cantidad))
                {
                    MessageBox.Show("Cantidad inválida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a cero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _productoService.AjustarStock(_productoSeleccionado.Id, -cantidad);
                CargarInventario();
                RefrescarSeleccion();
                TxtCantidadAjuste.Text = string.Empty;

                MessageBox.Show("Salida de stock registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefrescarSeleccion()
        {
            if (_productoSeleccionado == null)
                return;

            var actualizado = _productoService.ObtenerPorId(_productoSeleccionado.Id);
            if (actualizado == null)
                return;

            _productoSeleccionado = actualizado;
            TxtProductoSeleccionado.Text = actualizado.Nombre;
            TxtStockActual.Text = actualizado.Stock.ToString();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            _productoSeleccionado = null;
            TxtProductoSeleccionado.Text = string.Empty;
            TxtStockActual.Text = string.Empty;
            TxtCantidadAjuste.Text = string.Empty;
            DgInventario.SelectedItem = null;
        }
    }
}